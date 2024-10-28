﻿using MySql.Data.MySqlClient;
using System.Collections.Generic;
using SalesInventorySystem_WAM1.Models;
using System.Security.Cryptography;
using System.Text;

namespace SalesInventorySystem_WAM1.Handlers
{
    internal class DatabaseHandler
    {
        private readonly string connection_string;

        public DatabaseHandler(
            string db_name = "sales_inventory_system",
            string host = "localhost",
            int port = 3306,
            string username = "root",
            string password = ""
        )
        {
            this.connection_string =
                $"server={host};port={port};uid={username};pwd={password};database={db_name}";
        }

        /// <summary>
        /// Get a new connection to the database.
        /// </summary>
        /// <returns>A new MySQL connection.</returns>
        public MySqlConnection GetNewConnection() => new MySqlConnection(connection_string);

        /// <summary>
        /// Encrypt a password using SHA-256.
        /// </summary>
        /// <param name="password">The plaintext version of the password.</param>
        /// <returns>The SHA-256 hash of the password.</returns>
        public static string EncryptPassword(string password)
        {
            var c = new SHA256Managed();
            var h = new StringBuilder();
            byte[] hpw = c.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (byte b in hpw) h.Append(b.ToString("x2"));
            return h.ToString();
        }

        /// <summary>
        /// Log in a user using their given username and password.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A usermodel.</returns>
        public User Login(string username, string password)
        {
            using (var connection = GetNewConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var hashed_pass = EncryptPassword(password);
                    command.CommandText = "SELECT * FROM users WHERE username = @username AND userpass = @password";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", hashed_pass);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        return new User(
                            reader.GetInt32("id"),
                            reader.GetString("username"),
                            reader.GetString("userpass"),
                            reader.IsDBNull(reader.GetOrdinal("name")) ? string.Empty : reader.GetString("name"),
                            reader.GetString("role")
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Get all users in the database.
        /// </summary>
        /// <returns>A list of users in the database.</returns>
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            using (MySqlConnection connection = GetNewConnection())
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(
                                new User(
                                    reader.GetInt32("id"),
                                    reader.GetString("username"),
                                    reader.GetString("userpass"),
                                    reader.IsDBNull(reader.GetOrdinal("name")) ? string.Empty : reader.GetString("name"),
                                    reader.GetString("role")
                                )
                            );
                        }
                    }
                }
            }
            return users;
        }

        public User GetUser(int user_id)
        {
            using (MySqlConnection connection = GetNewConnection())
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users WHERE id = @user_id";
                    command.Parameters.AddWithValue("@user_id", user_id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User(
                                reader.GetInt32("id"),
                                reader.GetString("username"),
                                reader.GetString("userpass"),
                                reader.IsDBNull(reader.GetOrdinal("name")) ? string.Empty : reader.GetString("name"),
                                reader.GetString("role")
                            );
                        }
                        return null;
                    }
                }
            }
        }

        public void DeleteUser(int user_id)
        {
            using (MySqlConnection connection = GetNewConnection())
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM users WHERE id = @user_id";
                    command.Parameters.AddWithValue("@user_id", user_id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
