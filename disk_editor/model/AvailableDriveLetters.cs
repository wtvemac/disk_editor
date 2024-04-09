using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LTR.IO.ImDisk;
using System.IO;
using System.Collections.Specialized;

namespace disk_editor
{
    class AvailableDriveLetters
    {
        char[] all_drive_letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 
                                     'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 
                                     'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        public StringCollection get_available_drive_letters()
        {
            var available_drive_letters = new StringCollection();
            var existing_drives = DriveInfo.GetDrives();

            foreach (char drive_letter in all_drive_letters)
            {
                available_drive_letters.Add(drive_letter.ToString().ToUpper());
            }

            foreach (DriveInfo existing_drive in existing_drives)
            {
                available_drive_letters.Remove(existing_drive.Name.Substring(0, 1).ToUpper());
            }

            return available_drive_letters;
        }
    }
}
