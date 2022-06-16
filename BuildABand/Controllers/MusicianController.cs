﻿using BuildABand.DAL;
using BuildABand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace BuildABand.Controllers
{
    /// <summary>
    /// This class serves as the controller
    /// for data related to Musician table in DB.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MusicianController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly MusicianDAL userSource;

        public MusicianController(IConfiguration configuration)

        /// <summary>
        /// 1-param constructor.
        /// </summary>
        /// <param name="configuration"></param>
        public MusicianController(IConfiguration configuration)
        {
            _configuration = configuration;
            this.userSource = new MusicianDAL(_configuration);
        }

        /// <summary>
        /// Gets all musicians
        /// GET: api/musicians
        /// </summary>
        /// <returns>JsonResult table of all musicians</returns>
        [HttpGet]
        public JsonResult Get()
        {
            string selectStatement = @"SELECT * 
                                    FROM dbo.Musician";

            DataTable resultsTable = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("BuildABandAppCon");
            SqlDataReader dataReader;
            using (SqlConnection connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (SqlCommand myCommand = new SqlCommand(selectStatement, connection))
                {
                    dataReader = myCommand.ExecuteReader();
                    resultsTable.Load(dataReader);
                    dataReader.Close();
                    connection.Close();
                }
            }

            return new JsonResult(resultsTable);
        }

        // Post: api/musician
        // Post new musician
        [HttpPost]
        public JsonResult PostNewMusician(NewMusician user)
        {
            try
            {
                this.userSource.RegisterNewUser(user);
            }
           catch (Exception)
            {
               return new JsonResult("Username already exist");
            }

            return new JsonResult("New user created");
        }
    }
}