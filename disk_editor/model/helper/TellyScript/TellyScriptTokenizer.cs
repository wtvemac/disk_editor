using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace disk_editor
{
    internal class TellyScriptTokenizer
    {
        public List<byte> tokenzed_data;
        public string raw_data;

        private int index;
        private int end_index;
        private int line_number;

        public const char INDENT_WHITESPACE_CHARATER = '\t';
        public const char SEPARATOR_WHITESPACE_CHARACTER = ' ';
        public const char NEWLINE_WHITESPACE_CHARACTER = '\n';
        public const char STRING_DELIMITER = '"';
        public const char CONSTANT_DELIMITER = '\'';
        public const char ESCAPE_MARKER = '\\';
        public const string COMMENT_SEQUENCE_START = "/*";
        public const string COMMENT_SEQUENCE_END = "*/";
        public const char EMPTY = '\0';

        private IDictionary<string, byte> tokenizer_replacements;

        public char consume_escape_sequence()
        {
            var character = this.get_next_character();

            switch (character)
            {
                case 'a':
                    return (char)0x07 /* BEL */;

                case 'b':
                    return (char)0x08 /* BS */;

                case 't':
                    return (char)0x09 /* TAB */;

                case 'n':
                    return (char)0x0A /* LF */;

                case 'v':
                    return (char)0x0B /* VT */;

                case 'f':
                    return (char)0x0C /* FF */;

                case 'r':
                    return (char)0x0D /* CR */;

                case 'x':
                case 'X':
                    var digit1 = character.ToString();
                    var digit2 = get_next_character().ToString();

                    return (char)UInt32.Parse(digit1 + digit2, System.Globalization.NumberStyles.HexNumber);

                default:
                    return character;
            }
        }

        public void tokenize_constant_number(byte[] constant_digits)
        {
            this.tokenzed_data.Add(0x43 /* 'C' */);
            var past_leading_zerors = false;
            for(var i = 0; i < constant_digits.Length; i++)
            {
                if(past_leading_zerors || constant_digits[i] != 0x00)
                {
                    past_leading_zerors = true;
                    this.tokenzed_data.Add((byte)(constant_digits[i] + 0x30 /* 0 */));
                }
            }
            this.tokenzed_data.Add((byte)0x00);
        }

        public void tokenize_constant()
        {

            var constant_bytes = new List<byte>();
            while (this.index <= this.end_index)
            {
                var character = get_next_character();

                if (character != CONSTANT_DELIMITER)
                { 
                    if (character == ESCAPE_MARKER)
                    {
                        character = this.consume_escape_sequence();
                    }

                    constant_bytes.Add((byte)character);
                }
                else
                {
                    break;
                }
            }

            this.tokenize_constant_number(constant_bytes.ToArray());
        }

        public void tokenize_string()
        {
            this.tokenzed_data.Add(0x53 /* 'S' */);

            while (this.index <= this.end_index)
            {
                var character = get_next_character();

                if (character != STRING_DELIMITER)
                {
                    if (character == ESCAPE_MARKER)
                    {
                        character = this.consume_escape_sequence();
                    }

                    this.tokenzed_data.Add((byte)character);
                }
                else
                {
                    break;
                }
            }

            this.tokenzed_data.Add((byte)0x00);
        }

        public void tokenize_identifier_or_constant(string check_sequence)
        {
            if (Regex.IsMatch(check_sequence, "^[a-zA-Z_]"))
            {
                this.tokenzed_data.Add(0x49 /* 'I' */);
                this.tokenzed_data.AddRange(Encoding.Default.GetBytes(check_sequence));
                this.tokenzed_data.Add((byte)0x00);
            }
            else
            {
                var match = Regex.Match(check_sequence, "^(0x|)([0-9a-fA-F]+)$");

                if(match.Success)
                {
                    var constant_bytes = new List<byte>();

                    var hex_string = "";

                    if (match.Groups[1].Value == "0x")
                    {
                        hex_string = match.Groups[2].Value;
                    }
                    else
                    {
                        hex_string = UInt32.Parse(match.Groups[2].Value).ToString("X");
                    }

                    for (var i = 0; i < hex_string.Length; i++)
                    {
                        constant_bytes.Add((byte)UInt16.Parse(hex_string[i].ToString(), System.Globalization.NumberStyles.HexNumber));
                    }

                    this.tokenize_constant_number(constant_bytes.ToArray());
                }
                else
                {
                    throw new Exception("Invalid constant '" + check_sequence + "' at line '" + this.line_number + "'.");
                }
            }
        }

        public char get_next_character()
        {
            if (this.index <= this.end_index)
            {
                var character = this.raw_data[this.index];
                this.index++;

                return character;

            }
            else
            {
                return '?';
            }
        }

        public string build_check_sequence(char first_character, string regex)
        {
            string check_sequence = "";

            if (Regex.IsMatch(Convert.ToChar(first_character).ToString(), regex))
            {
                check_sequence += Convert.ToChar(first_character).ToString();

                while (this.index <= this.end_index)
                {
                    var character_next = get_next_character();

                    if (Regex.IsMatch(Convert.ToChar(character_next).ToString(), regex))
                    {
                        check_sequence += Convert.ToChar(character_next).ToString();
                    }
                    else
                    {
                        this.index--;
                        break;
                    }
                }
            }

            return check_sequence;
        }

        public byte[] tokenize()
        {
            this.tokenzed_data = new List<byte>();

            this.line_number = 1;
            this.index = 0;
            this.end_index = this.raw_data.Length - 1;
            while (this.index <= this.end_index)
            {
                var character = get_next_character();

                // Remove comments
                if (character == COMMENT_SEQUENCE_START[0])
                {
                    var character2 = get_next_character();
                    if (character2 != COMMENT_SEQUENCE_START[1])
                    {
                        this.index--;
                    }
                    else
                    {
                        while (this.index <= this.end_index)
                        {
                            var character3 = get_next_character().ToString();
                            var character4 = get_next_character().ToString();

                            if ((character3 + character4) == COMMENT_SEQUENCE_END)
                            {
                                break;
                            }
                            else
                            {
                                this.index--;
                            }
                        }

                        continue;
                    }
                }

                // Remove whitespace
                if (character == SEPARATOR_WHITESPACE_CHARACTER || character == INDENT_WHITESPACE_CHARATER)
                {
                    continue;
                }
                else if (character == STRING_DELIMITER)
                {
                    this.tokenize_string();
                }
                else if (character == CONSTANT_DELIMITER)
                {
                    this.tokenize_constant();
                }
                else
                {
                    var current_index = this.index;

                    var check_sequence = this.build_check_sequence(character, "^[a-zA-Z0-9_]$");
                    if (check_sequence != "")
                    {
                        if (this.tokenizer_replacements.ContainsKey(check_sequence))
                        {
                            this.tokenzed_data.Add(this.tokenizer_replacements[check_sequence]);
                        }
                        else
                        {
                            this.tokenize_identifier_or_constant(check_sequence);
                        }
                    }
                    else
                    {
                        this.index = current_index;

                        check_sequence = this.build_check_sequence(character, "^[\\-+=<>!\\|\\&]$");
                        if (this.tokenizer_replacements.ContainsKey(check_sequence))
                        {
                            this.tokenzed_data.Add(this.tokenizer_replacements[check_sequence]);
                        }
                        else if (this.tokenizer_replacements.ContainsKey(character.ToString()))
                        {
                            var replacement = this.tokenizer_replacements[character.ToString()];

                            if (replacement == 0x7F)
                            {
                                this.line_number++;
                            }

                            this.tokenzed_data.Add(replacement);
                        }
                        else
                        {
                            throw new Exception("Invalid character '" + ((byte)character).ToString() + "' at line '" + this.line_number + "'.");
                        }
                    }
                }
            }

            this.tokenzed_data.Add(0xFF /* EOF */);

            return this.tokenzed_data.ToArray();
        }

        public void setup_replacements()
        {
            this.tokenizer_replacements = new Dictionary<string, byte>();

            this.tokenizer_replacements.Add("!", 0x21 /* '!' */);
            this.tokenizer_replacements.Add("%", 0x25 /* '%' */);
            this.tokenizer_replacements.Add("&&", 0x26 /* '&' */);
            this.tokenizer_replacements.Add("(", 0x28 /* '(' */);
            this.tokenizer_replacements.Add(")", 0x29 /* ')' */);
            this.tokenizer_replacements.Add("*", 0x2A /* '*' */);
            this.tokenizer_replacements.Add("+", 0x2B /* '+' */);
            this.tokenizer_replacements.Add(",", 0x2C /* ',' */);
            this.tokenizer_replacements.Add("-", 0x2D /* '-' */);
            this.tokenizer_replacements.Add("/", 0x2F /* '/' */);
            this.tokenizer_replacements.Add(";", 0x3B /* ';' */);
            this.tokenizer_replacements.Add("<", 0x3C /* '<' */);
            this.tokenizer_replacements.Add("=", 0x3D /* '=' */);
            this.tokenizer_replacements.Add(">", 0x3E /* '>' */);
            this.tokenizer_replacements.Add("&", 0x40 /* '@' */);
            this.tokenizer_replacements.Add("+=", 0x41 /* 'A' */);
            this.tokenizer_replacements.Add("-=", 0x42 /* 'B' */);
            this.tokenizer_replacements.Add("--", 0x44 /* 'D' */);
            this.tokenizer_replacements.Add("==", 0x45 /* 'E' */);
            this.tokenizer_replacements.Add(">=", 0x47 /* 'G' */);
            this.tokenizer_replacements.Add("<=", 0x4C /* 'L' */);
            this.tokenizer_replacements.Add("*=", 0x4D /* 'M' */);
            this.tokenizer_replacements.Add("!=", 0x4E /* 'N' */);
            this.tokenizer_replacements.Add("++", 0x50 /* 'P' */);
            this.tokenizer_replacements.Add("/=", 0x56 /* 'V' */);
            this.tokenizer_replacements.Add("[", 0x5B /* '[' */);
            this.tokenizer_replacements.Add("]", 0x5D /* ']' */);
            this.tokenizer_replacements.Add("break", 0x62 /* 'b' */);
            this.tokenizer_replacements.Add("char", 0x63 /* 'c' */);
            this.tokenizer_replacements.Add("else", 0x65 /* 'e' */);
            this.tokenizer_replacements.Add("for", 0x66 /* 'f' */);
            this.tokenizer_replacements.Add("if", 0x69 /* 'i' */);
            this.tokenizer_replacements.Add("int", 0x6C /* 'l' */);
            this.tokenizer_replacements.Add("return", 0x72 /* 'r' */);
            this.tokenizer_replacements.Add("while", 0x77 /* 'w' */);
            this.tokenizer_replacements.Add("{", 0x7B /* '{' */);
            this.tokenizer_replacements.Add("||", 0x7C /* '|' */);
            this.tokenizer_replacements.Add("}", 0x7D /* '}' */);
            this.tokenizer_replacements.Add(NEWLINE_WHITESPACE_CHARACTER.ToString(), 0x7F /* DEL (newline) */);
            this.tokenizer_replacements.Add("\r", 0x7F /* DEL (newline) */);
        }

        public TellyScriptTokenizer(string raw_data)
        {
            this.raw_data = raw_data;

            this.setup_replacements();
        }
    }
}
