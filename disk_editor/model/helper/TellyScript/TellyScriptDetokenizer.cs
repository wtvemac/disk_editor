using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    public enum WhitespaceInstruction
    {
        ADD_NONE = 0,
        ADD_BEFORE = 1,
        ADD_AFTER = 2,
        ADD_BOTH = 3,
        CHECK_NEWSCOPE1 = 4,
        CHECK_NEWSCOPE2 = 5,
        CHECK_IF_MATH = 6,
    };

    public enum ScopeCheckStep
    {
        OFF = 0,
        RB_DELIMITER_CHECK = 1,
        CB_DELIMITER_CHECK = 2,
        SINGLE_LINE_CHECK = 3,
        SINGLE_LINE_HIT = 4,
        MULTI_LINE_CHECK = 5
    };

    public class DetokenizerInstruction
    {
        public delegate void instruction_method();

        public instruction_method instruction { get; set; }
        public string output { get; set; } // empty string = echo back character
        public WhitespaceInstruction whitespace { get; set; }
        public ScopeCheckStep enter_scope_check { get; set; }
        public int move_cbracket_scope_amount { get; set; }
        public int move_rbracket_scope_amount { get; set; }
        public bool terminate { get; set; }
        public bool script_start_ignore { get; set; }
    }

    internal class TellyScriptDetokenizer
    {
        public const int USE_HEX_INTEGER_AFTER = 0x1000;
        //public const bool USE_CHAR_FOR_ONE_BYTE_COMPARISON = true;
        public const int INDENT_WHITESPACE_COUNT = 1;
        public const char INDENT_WHITESPACE_CHARATER = '\t';
        public const char SEPARATOR_WHITESPACE_CHARACTER = ' ';
        public const char NEWLINE_WHITESPACE_CHARACTER = '\n';

        public byte[] tokenzed_data;
        public string raw_data;
        private IDictionary<byte, DetokenizerInstruction> detokenizer_instructions;

        private int index;
        private int end_index;
        private bool script_started;
        private ScopeCheckStep scope_check_step;
        private int check_scope_cb_level;
        private int check_scope_rb_level;
        private int cbracket_scope_level;
        private int rbracket_scope_level;

        public void detokenize_constant()
        {
            int constant_value = 0;
            string alphanumeric_value = "";

            var ii = 0;
            for (this.index++; this.index < this.tokenzed_data.Length; this.index++, ii++)
            {
                if (this.tokenzed_data[this.index] == 0x00)
                {
                    break;
                }

                var digit = this.tokenzed_data[this.index] - 0x30 /* 0 */;

                if (digit >= 0 && digit <= 0x0F)
                {
                    constant_value = (constant_value << 4) + digit;
                }

                if (ii >= 1 && (ii % 2) == 1 && alphanumeric_value != null)
                {
                    var char_value = constant_value & 0xFF;

                    if (char_value >= 0x30 /* A */ && char_value <= 0x5A /* Z */)
                    //if (char_value >= 0x30 /* A */ && char_value <= 0x7A /* z */)
                    {
                            alphanumeric_value += Convert.ToChar(char_value);
                    }
                    else
                    {
                        alphanumeric_value = null;
                    }
                }
            }

            if(alphanumeric_value == "")
            {
                alphanumeric_value = null;
            }

            if (constant_value >= 0 && constant_value < USE_HEX_INTEGER_AFTER)
            {
                /*var is_comparison = false;
                if (this.index >= 5)
                {
                    var _token = this.tokenzed_data[this.index - 4];

                    if (_token == 0x3C // '<'
                    || _token == 0x3E  // '>'
                    || _token == 0x45  // 'E' ==
                    || _token == 0x47  // 'G' >=
                    || _token == 0x4C  // 'L' <=
                    || _token == 0x4E) // 'N' !=
                    {
                        is_comparison = true;
                    }
                }*/


                /*if (is_comparison && USE_CHAR_FOR_ONE_BYTE_COMPARISON && alphanumeric_value != null && alphanumeric_value.Length == 1)
                {
                    this.raw_data += "'" + alphanumeric_value + "'";
                }
                else
                {*/
                this.raw_data += constant_value.ToString();
                //}
            }
            else
            {
                this.raw_data += "0x" + constant_value.ToString("X");

                if (alphanumeric_value != null)
                {
                    this.raw_data += " /* " + alphanumeric_value + " */";
                }
            }
        }

        public void detokenize_string()
        {
            var count = 0;
            var index = ++this.index;
            while (this.index <= this.end_index)
            {
                if (this.tokenzed_data[this.index] == 0x00)
                {
                    break;
                }

                count++;
                this.index++;
            }

            var _string = Encoding.GetEncoding(932).GetString(this.tokenzed_data, index, count);

            _string = _string.Replace("\x07", "\\a"); /* BEL */
            _string = _string.Replace("\x08", "\\b"); /* BS */
            _string = _string.Replace("\x09", "\\t"); /* TAB */
            _string = _string.Replace("\x0A", "\\n"); /* LF */
            _string = _string.Replace("\x0B", "\\v"); /* VT */
            _string = _string.Replace("\x0C", "\\f"); /* FF */
            _string = _string.Replace("\x0D", "\\r"); /* CR */

            this.raw_data += '"';
            this.raw_data += _string;
            this.raw_data += '"';
        }

        public void detokenize_identifier()
        {
            for (this.index++; this.index < this.tokenzed_data.Length; this.index++)
            {
                if (this.tokenzed_data[this.index] == 0x00)
                {
                    break;
                }

                this.raw_data += Convert.ToChar(this.tokenzed_data[this.index]);
            }
        }

        public void detokenize_newline()
        {
            /* Reduce newline clutter by enforcing 2 newlines max.
             Ruins the original line count but it shouldn't matter. */
            if (this.index >= 2
            && this.tokenzed_data[this.index - 1] == 0x7F /* DEL (newline) */
            && this.tokenzed_data[this.index - 2] == 0x7F /* DEL (newline) */)
            {
                return;
            }
            // ignore newlines at the begining before the actual script starts.
            if(!this.script_started)
            {
                return;
            }

            this.raw_data += NEWLINE_WHITESPACE_CHARACTER;

            var whitespace_amount = this.cbracket_scope_level * INDENT_WHITESPACE_COUNT;
            if (this.index < this.end_index && this.tokenzed_data[this.index + 1] == 0x7D /* } */)
            {
                whitespace_amount -= INDENT_WHITESPACE_COUNT;
            }
            else if (this.scope_check_step == ScopeCheckStep.SINGLE_LINE_CHECK)
            {
                whitespace_amount += INDENT_WHITESPACE_COUNT;
                this.scope_check_step = ScopeCheckStep.SINGLE_LINE_HIT;
            }


            for (var ii = 0; ii < whitespace_amount; ii++)
            {
                this.raw_data += INDENT_WHITESPACE_CHARATER;
            }
        }

        public string detokenize()
        {
            this.raw_data = "";

            this.index = 0;
            this.end_index = this.tokenzed_data.Length - 1;
            while (this.index <= end_index)
            {
                var token = this.tokenzed_data[this.index];

                if (this.detokenizer_instructions.ContainsKey(token))
                {
                    var instruction_info = this.detokenizer_instructions[token];

                    if (instruction_info.terminate)
                    {
                        break;
                    }

                    if (instruction_info.instruction != null)
                    {
                        instruction_info.instruction.Invoke();
                    }
                    else
                    { 
                        string _out = "";

                        if (instruction_info.output == null || instruction_info.output == "")
                        {
                            _out += Convert.ToChar(token);
                        }
                        else
                        {
                            _out += instruction_info.output;
                        }

                        if (instruction_info.whitespace == WhitespaceInstruction.ADD_BEFORE)
                        {
                            _out = SEPARATOR_WHITESPACE_CHARACTER + _out;
                        }
                        else if (instruction_info.whitespace == WhitespaceInstruction.ADD_AFTER)
                        {
                            _out += SEPARATOR_WHITESPACE_CHARACTER;
                        }
                        else if (instruction_info.whitespace == WhitespaceInstruction.ADD_BOTH)
                        {
                            _out = SEPARATOR_WHITESPACE_CHARACTER + _out + SEPARATOR_WHITESPACE_CHARACTER;
                        }
                        else if (instruction_info.whitespace == WhitespaceInstruction.CHECK_NEWSCOPE1)
                        {
                            if (this.index >= 2 && this.tokenzed_data[this.index - 1] == 0x7D /* } */)
                            {
                                _out = SEPARATOR_WHITESPACE_CHARACTER + _out + SEPARATOR_WHITESPACE_CHARACTER;
                            }
                            else
                            {
                                _out = _out + SEPARATOR_WHITESPACE_CHARACTER;
                            }
                        }
                        else if (instruction_info.whitespace == WhitespaceInstruction.CHECK_NEWSCOPE2)
                        {
                            if (this.index >= 2 && this.tokenzed_data[this.index - 1] == 0x29 /* ) */)
                            {
                                _out = SEPARATOR_WHITESPACE_CHARACTER + _out;
                            }
                        }
                        else if (instruction_info.whitespace == WhitespaceInstruction.CHECK_IF_MATH)
                        {
                            if (this.index >= 2 && this.tokenzed_data[this.index - 1] == 0x00 /* NULL = end of character group */)
                            {
                                _out = " " + _out + " ";
                            }
                        }

                        this.cbracket_scope_level += instruction_info.move_cbracket_scope_amount;
                        this.rbracket_scope_level += instruction_info.move_rbracket_scope_amount;

                        // Scope check (for, if, while) started and the round brackets finished. Code need to follow.
                        if (this.scope_check_step == ScopeCheckStep.RB_DELIMITER_CHECK
                        && instruction_info.move_rbracket_scope_amount < 0
                        && this.rbracket_scope_level == this.check_scope_rb_level)
                        {
                            this.scope_check_step = ScopeCheckStep.CB_DELIMITER_CHECK;
                        }
                        else if (instruction_info.enter_scope_check != ScopeCheckStep.OFF)
                        {
                            this.scope_check_step = instruction_info.enter_scope_check;
                            this.check_scope_cb_level = this.cbracket_scope_level;
                            this.check_scope_rb_level = this.rbracket_scope_level;
                        }

                        if (this.scope_check_step == ScopeCheckStep.CB_DELIMITER_CHECK)
                        {
                            if (instruction_info.move_cbracket_scope_amount > 0)
                            {
                                this.scope_check_step = ScopeCheckStep.MULTI_LINE_CHECK;
                            }
                            // We're in a scope check and the +1 and +2 character isn't a newline or a open curley bracket so assuming single-line code.
                            else if ((this.index + 1) < this.end_index
                            && (   (this.tokenzed_data[this.index + 1] != 0x7F /* DEL (newline) */ && this.tokenzed_data[this.index + 1] != 0x7B /* { */)
                                || (this.tokenzed_data[this.index + 2] != 0x7F /* DEL (newline) */ && this.tokenzed_data[this.index + 2] != 0x7B /* { */)))
                            {
                                this.scope_check_step = ScopeCheckStep.SINGLE_LINE_CHECK;
                            }
                        }
                        // Exit scope if the line has finished.
                        else if (this.scope_check_step == ScopeCheckStep.SINGLE_LINE_CHECK && token != 0x7F /* DEL (newline) */)
                        {
                            this.scope_check_step = ScopeCheckStep.SINGLE_LINE_HIT;
                        }
                        else if (this.scope_check_step == ScopeCheckStep.SINGLE_LINE_HIT
                        && token == 0x7F /* DEL (newline) */)
                        {
                            this.scope_check_step = ScopeCheckStep.OFF;
                        }
                        else if (this.scope_check_step == ScopeCheckStep.MULTI_LINE_CHECK
                        && instruction_info.move_cbracket_scope_amount < 0
                        && this.cbracket_scope_level == this.check_scope_cb_level)
                        {
                            this.scope_check_step = ScopeCheckStep.OFF;
                        }

                        this.raw_data += _out;
                    }

                    if(!instruction_info.script_start_ignore)
                    {
                        this.script_started = true;
                    }
                }
                else
                {
                    // ignore tokens we don't have instructions for.
                    this.raw_data += "/*?0x" + token.ToString("X") + "?*/";
                }

                this.index++;
            }

            return this.raw_data;
        }

        public void setup_instructions()
        {
            this.detokenizer_instructions = new Dictionary<byte, DetokenizerInstruction>();

            this.detokenizer_instructions.Add(
                0x21 /* '!' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x25 /* '%' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x26 /* '&' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "&&",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x28 /* '(' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 1
                }
            );
            this.detokenizer_instructions.Add(
                0x29 /* ')' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = -1
                }
            );
            this.detokenizer_instructions.Add(
                0x2A /* '*' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.CHECK_IF_MATH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x2B /* '+' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x2C /* ',' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_AFTER,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x2D /* '-' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x2F /* '/' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x3B /* ';' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x3C /* '<' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x3D /* '=' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x3E /* '>' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x40 /* '@' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "&",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x41 /* 'A' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "+=",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x42 /* 'B' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "-=",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x43 /* 'C' */,
                new DetokenizerInstruction()
                {
                    instruction = this.detokenize_constant,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x44 /* 'D' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "--",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x45 /* 'E' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "==",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x47 /* 'G' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = ">=",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x49 /* 'I' */,
                new DetokenizerInstruction()
                {
                    instruction = this.detokenize_identifier,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x4C /* 'L' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "<=",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x4D /* 'M' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "*=",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x4E /* 'N' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "!=",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x50 /* 'P' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "++",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x53 /* 'S' */,
                new DetokenizerInstruction()
                {
                    instruction = this.detokenize_string,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x56 /* 'V' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "/=",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x5B /* '[' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x5D /* ']' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x62 /* 'b' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "break",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x63 /* 'c' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "char",
                    whitespace = WhitespaceInstruction.ADD_AFTER,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x65 /* 'e' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "else",
                    whitespace = WhitespaceInstruction.CHECK_NEWSCOPE1,
                    enter_scope_check = ScopeCheckStep.CB_DELIMITER_CHECK,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x66 /* 'f' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "for",
                    whitespace = WhitespaceInstruction.ADD_AFTER,
                    enter_scope_check = ScopeCheckStep.RB_DELIMITER_CHECK,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x69 /* 'i' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "if",
                    whitespace = WhitespaceInstruction.ADD_AFTER,
                    enter_scope_check = ScopeCheckStep.RB_DELIMITER_CHECK,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x6C /* 'l' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "int",
                    whitespace = WhitespaceInstruction.ADD_AFTER,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x72 /* 'r' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "return",
                    whitespace = WhitespaceInstruction.ADD_AFTER,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x77 /* 'w' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "while",
                    whitespace = WhitespaceInstruction.ADD_AFTER,
                    enter_scope_check = ScopeCheckStep.RB_DELIMITER_CHECK,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x7B /* '{' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.CHECK_NEWSCOPE2,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 1,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x7C /* '|' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "||",
                    whitespace = WhitespaceInstruction.ADD_BOTH,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x7D /* '}' */,
                new DetokenizerInstruction()
                {
                    instruction = null,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = -1,
                    move_rbracket_scope_amount = 0
                }
            );
            this.detokenizer_instructions.Add(
                0x7F /* DEL (newline) */,
                new DetokenizerInstruction()
                {
                    instruction = this.detokenize_newline,
                    script_start_ignore = true,
                    output = "",
                    whitespace = WhitespaceInstruction.ADD_NONE,
                    enter_scope_check = ScopeCheckStep.OFF,
                    move_cbracket_scope_amount = 0,
                    move_rbracket_scope_amount = 0
                }
            ); ;
            this.detokenizer_instructions.Add(
                0xFF /* = EOF */,
                new DetokenizerInstruction()
                {
                    terminate = true
                }
            );
        }

        public TellyScriptDetokenizer(byte[] tokenzed_data)
        {
            this.tokenzed_data = tokenzed_data;

            this.setup_instructions();
        }
    }
}
