﻿namespace Naos.KeyValueStorage.Infrastructure.Azure
{
    using Naos.Foundation;

    public class TableKeyValueStorageOptions : OptionsBase
    {
        public string ConnectionString { get; set; }

        public int MaxInsertLimit { get; set; } = 100;
    }
}
