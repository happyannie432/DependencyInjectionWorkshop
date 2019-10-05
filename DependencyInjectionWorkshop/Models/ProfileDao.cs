﻿using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPasswordFromDb(string accountId)
        {
            string password;
            using (var connection = new SqlConnection("my connection string"))
            {
                password = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return password;
        }
    }
}