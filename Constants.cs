using System;
using Microsoft.Azure.WebJobs.Extensions.Timers;

namespace Com.ZoneIct
{
    public static class Constants
    {
        static Constants()
        {
        }
        public const string Version = "0.9.0";
        public const string ImagePath = "https://storageaccountlinea81cb.blob.core.windows.net/public-contents/image/";
        public const string CosmosDB = "zone-ict-app";
        public const string CosmosDBConnection = "COSMOSDB_CONNECTION";
    }
}