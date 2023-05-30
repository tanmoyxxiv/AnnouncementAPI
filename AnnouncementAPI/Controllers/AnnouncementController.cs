﻿using AnnouncementAPI.Common.Constants;
using AnnouncementAPI.Models;
using LPL.LoggingDemo.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data.SqlClient;
using System.Data;
using AnnouncementAPI.Models.DTO;

namespace AnnouncementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : BaseController
    {
        public AnnouncementController() : base()
        {
        }

        //HTTP GET method to fetch records from the database
        [HttpGet("/getAnnouncement")]
        public IActionResult GetAnnouncements()
        {
            try
            {
                using (SqlConnection connection = GetSqlConnection(DatabaseName.dbName)) //establishing connection to the database
                {
                    var announcements = new List<GetAnnouncement>(); //list to store Announcement records

                    using (SqlCommand command = new SqlCommand(StoredProcedure.getAnnouncement, connection)) //creating a new SQL command object using the stored procedure name and the connection object
                    {
                        command.CommandType = CommandType.StoredProcedure; //setting the command type to stored procedure

                        using (SqlDataReader reader = command.ExecuteReader()) //executing the command and creating a data reader object
                        {
                            while (reader.Read()) //reading each record from the data reader
                            {
                                announcements.Add(new GetAnnouncement() //creating a new GetAnnouncement DTO object with the values from the record and adding it to the list
                                {
                                    Announcement_ID = Convert.ToInt32(reader["Announcement_ID"]),
                                    UserID = Convert.ToInt32(reader["UserID"]),
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Title = reader["Title"].ToString(),
                                    Summary = reader["Summary"].ToString(),
                                    Descriptions = reader["Descriptions"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"].ToString())


                                });
                            }
                        }
                    }


                    return Ok(announcements); //returning the list of announcements as an HTTP response with status code 200 (OK)
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message); //logging the error message using Serilog
                Log.Error(ex.ToString()); //logging the error message using Serilog
                return StatusCode(500); //returning an HTTP response with status code 500 (Internal Server Error)
            }
        }


        //HTTP POST method to save announcement records to the database
        [HttpPost("/saveAnnouncement")]
        public IActionResult SaveAnnouncement(Announcement announcement, string actionPerformed)
        {
            try
            {
                using (SqlConnection connection = GetSqlConnection(DatabaseName.dbName)) //establishing connection to the database
                {

                    using (SqlCommand command = new SqlCommand(StoredProcedure.saveAnnouncement, connection)) //creating a new SQL command object using the stored procedure name and the connectionobject
                    {
                        command.CommandType = CommandType.StoredProcedure; //setting the command type to stored procedure
                        command.Parameters.AddWithValue("@Announcement_ID", announcement.Announcement_ID);
                        command.Parameters.AddWithValue("@UserID", announcement.UserID); //adding parameters to the command object
                        command.Parameters.AddWithValue("@Title", announcement.Title);
                        command.Parameters.AddWithValue("@Summary", announcement.Summary);
                        command.Parameters.AddWithValue("@Descriptions", announcement.Descriptions);
                        command.Parameters.AddWithValue("@CreatedDate", announcement.CreatedDate);
                        command.Parameters.AddWithValue("@Action", actionPerformed);

                        int i = command.ExecuteNonQuery(); //executing the command and getting the number of affected rows

                        if (i >= 1) //if at least one row is affected
                            return Ok("Announcement " + actionPerformed + " action Performed Successfully."); //returning an HTTP response with status code 200 (OK) and a success message
                        else
                        {
                            return BadRequest("Action Failed"); //returning an HTTP response with status code 400 (Bad Request) and an error message
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message); //logging the error message using Serilog
                return StatusCode(500); //returning an HTTP response with status code 500 (Internal Server Error)
            }
        }
    }
}
