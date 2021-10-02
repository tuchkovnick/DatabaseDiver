using DbDiver.Core;
using DbDiver.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbDiver.Business
{
    public static class DbSearcher
    {
        public static bool CheckValueExist(DbSearchParameter parameter, string connectionString)
        {
            DatabaseSearcher ds = new DatabaseSearcher(connectionString);
            return ds.HasParameter(parameter);
        }
    }
}
