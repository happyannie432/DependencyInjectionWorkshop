﻿using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPassword(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public string GetPassword(string accountId)
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