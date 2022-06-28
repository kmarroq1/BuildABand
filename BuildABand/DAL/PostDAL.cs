using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BuildABand.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BuildABand.DAL
{
    /// <summary>
    /// Post table data access layer
    /// </summary>
    public class PostDAL
    {
        private IConfiguration _configuration;
        public PostDAL(IConfiguration configuration){
            _configuration = configuration;
        }

        public List<Post> GetPostByMusicianID(int musicianID)
        {
            List<Post> posts = new List<Post>();
            string selectStatement = 
                @"SELECT *
                FROM dbo.Post 
                WHERE musicianID = @id";

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BuildABandAppCon")))
            {
                connection.Open();
                using (SqlCommand selectCommand = new SqlCommand(selectStatement, connection))
                {
                    selectCommand.Parameters.AddWithValue("@id", musicianID);
                     using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return null;
                        }

                        while (reader.Read())
                        {
                            Post post = new Post
                            {
                                PostID = (int)reader["postID"],
                                CreatedTime = (DateTime)reader["createdTime"],
                                MusicianID = (int)reader["musicianID"],
                                Content = reader["content"].ToString(),
                            };

                        posts.Add(post);
                        }
                    }
                    connection.Close();
                }
            }
            return posts;
        }

        /// <summary>
        /// Removes Post row from table,
        /// and its associated likes and comments.
        /// </summary>
        /// <param name="postID"></param>
        /// <returns>JsonResult with deletion status</returns>
        public JsonResult DeletePostByID(int postID)
        {
            if (!this.PostExists(postID))
            {
                throw new ArgumentException("Error: post does not exist");
            }

            ///Statement deletes any row from Post, PostLike, Comment, CommentLike connected to @PostID
            string deleteStatement = @"
            DELETE FROM dbo.PostLike
            WHERE PostID = @PostID
            DECLARE @lastPostLikeID int
            SELECT @lastPostLikeID = MAX(PostLikeID) FROM dbo.PostLike
            IF @lastPostLikeID IS NULL
            DBCC CHECKIDENT(PostLike, RESEED, 0)
            ELSE
            DBCC CHECKIDENT(PostLike, RESEED, @lastPostLikeID)

            DELETE FROM dbo.CommentLike
            WHERE CommentID IN (SELECT CommentID FROM Comment WHERE PostID = @PostID)
            DECLARE @lastCommentLikeID int
            SELECT @lastCommentLikeID = MAX(CommentLikeID) FROM dbo.CommentLike
            IF @lastCommentLikeID IS NULL
            DBCC CHECKIDENT(CommentLike, RESEED, 0)
            ELSE 
            DBCC CHECKIDENT(CommentLike, RESEED, @lastCommentLikeID)

            DELETE FROM dbo.Comment
            WHERE PostID = @PostID
            DECLARE @lastCommentID int
            SELECT @lastCommentID = MAX(CommentID) FROM dbo.Comment
            IF @lastCommentID IS NULL 
            DBCC CHECKIDENT(Comment, RESEED, 0)
            ELSE
            DBCC CHECKIDENT(Comment, RESEED, @lastCommentID)

            DELETE FROM dbo.Post
            WHERE PostID = @PostID
            DECLARE @lastPostID int
            SELECT @lastPostID = MAX(PostID) FROM dbo.Post
            IF @lastPostID IS NULL
            DBCC CHECKIDENT(Post, RESEED, 0)
            ELSE
            DBCC CHECKIDENT(Post, RESEED, @lastPostID)
            ";

            DataTable resultsTable = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("BuildABandAppCon");
                SqlDataReader dataReader;
                using (SqlConnection connection = new SqlConnection(sqlDataSource))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        using (SqlCommand myCommand = new SqlCommand(deleteStatement, connection, transaction))
                        {
                            myCommand.Parameters.AddWithValue("@PostID", postID);
                            dataReader = myCommand.ExecuteReader();
                            resultsTable.Load(dataReader);
                            dataReader.Close();
                            transaction.Commit();
                            connection.Close();
                        }
                }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception(ex.Message);
                    }
            }

            return new JsonResult("Post Deleted Successfully");       
        }

        /// <summary>
        /// Returns true if post exists.
        /// </summary>
        /// <param name="postID"></param>
        /// <returns>True if post exists</returns>
        public bool PostExists(int postID)
        {
            if (postID <= 0)
            {
                throw new ArgumentException("Error: post ID must be greater than 0");
            }

            try
            {
                string selectStatement =
                    "SELECT COUNT(*) " +
                    "FROM dbo.Post " +
                    "WHERE PostID = @PostID";

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("BuildABandAppCon")))
                {
                    connection.Open();
                    using (SqlCommand selectCommand = new SqlCommand(selectStatement, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@PostID", postID);
                        bool postExists = Convert.ToBoolean(selectCommand.ExecuteScalar());

                        return postExists;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}