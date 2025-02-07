﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;

namespace SecretHistories.Entities
{
    [FucineImportable("cultures")]
    public class Culture : AbstractEntity<Culture>
    {
        [FucineValue]
        public string Endonym { get; set; }

        [FucineValue]
        public string Exonym { get; set; }

        [FucineValue(DefaultValue = "x")]
        public string FontScript { get; set; }

        [FucineValue]
        public bool BoldAllowed { get; set; }

        [FucineValue(DefaultValue = false)]
        public bool Released { get; set; }


        [FucineDict]
        public Dictionary<string,string> UILabels { get; set; }



        protected override void OnPostImportForSpecificEntity(ContentImportLog log, Compendium populatedCompendium)
        {
            var caseInsensitiveDictionary = new Dictionary<string, string>(UILabels, StringComparer.OrdinalIgnoreCase);
            UILabels = caseInsensitiveDictionary;
        }

        public Culture(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {

        }

    }
}
