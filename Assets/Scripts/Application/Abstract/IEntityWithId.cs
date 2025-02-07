﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;

namespace SecretHistories.Fucine
{

    public interface  IEntityWithId
    {
        string Id { get; }
        string UniqueId { get; }
        void SetId(string id);
        string Lever { get; }
        void OnPostImport(ContentImportLog log, Compendium populatedCompendium);
    }
}
