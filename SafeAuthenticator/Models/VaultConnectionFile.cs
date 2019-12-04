﻿using System;

namespace SafeAuthenticator.Models
{
    public class VaultConnectionFile
    {
        public string FileName { get; set; }

        public int FileId { get; set; }

        public DateTime AddedOn { get; set; }

        public bool IsActive { get; set; }
    }
}
