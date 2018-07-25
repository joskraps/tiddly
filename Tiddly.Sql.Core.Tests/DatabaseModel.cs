﻿using System;

namespace Tiddly.Sql.Core.Tests
{
    public class DatabaseModel
    {
        public Guid BrokerGuid { get; set; }

        public DateTime CreateDate { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool ReadOnly { get; set; }
    }
}