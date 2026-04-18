using Dapper;
using Microsoft.Data.SqlClient;
using semproject.Models;
using semproject.Repositories.Interfaces;
using System.Data;

namespace semproject.Repositories.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IDbConnection _db;
        private readonly IConfiguration _configuration;

        public AdminRepository(IDbConnection db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public List<UserViewModel> GetAllUsers()
        {
            var users = new List<UserViewModel>();

            try
            {
                users = _db.Query<UserViewModel>(
                    "SELECT Id, UserName, Email FROM AspNetUsers ORDER BY UserName"
                ).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
            }

            return users;
        }

        public async Task<string> GetUserNameByIdAsync(string userId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT UserName FROM AspNetUsers WHERE Id = @UserId",
                    new { UserId = userId });
            }
        }

        public async Task DeleteUserDataAsync(string userId)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Start a transaction
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // DELETE ALL USER DATA IN CORRECT ORDER

                        await connection.ExecuteAsync(
                            "DELETE FROM Likes WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        // 2. Delete comments
                        await connection.ExecuteAsync(
                            "DELETE FROM Comments WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        // 3. Delete GroupMembers (before Groups)
                        await connection.ExecuteAsync(
                            "DELETE FROM GroupMembers WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        // 4. Delete Groups created by user
                        await connection.ExecuteAsync(
                            "DELETE FROM Groups WHERE createdbyid = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        // 5. Delete follows
                        await connection.ExecuteAsync(
                            "DELETE FROM Follows WHERE FollowerId = @UserId OR FollowingId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        // 6. Delete notifications - FIXED QUERY
                        await connection.ExecuteAsync(
                            "DELETE FROM Notifications WHERE UserId = @UserId or relatedUserid=@UserId", // OR SenderId/ReceiverId if different
                            new { UserId = userId },
                            transaction: transaction);

                        // 7. Delete posts (AFER deleting likes/comments that depend on posts)
                        await connection.ExecuteAsync(
                            "DELETE FROM Posts WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        // 8. Delete Identity related tables
                        await connection.ExecuteAsync(
                            "DELETE FROM AspNetUserRoles WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        await connection.ExecuteAsync(
                            "DELETE FROM AspNetUserLogins WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        await connection.ExecuteAsync(
                            "DELETE FROM AspNetUserClaims WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        await connection.ExecuteAsync(
                            "DELETE FROM AspNetUserTokens WHERE UserId = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        // 9. Finally delete the user
                        var rowsAffected = await connection.ExecuteAsync(
                            "DELETE FROM AspNetUsers WHERE Id = @UserId",
                            new { UserId = userId },
                            transaction: transaction);

                        if (rowsAffected > 0)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                            throw new InvalidOperationException("Failed to delete user.");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error deleting user: {ex.Message}", ex);
                    }
                }
            }
        }
    }
}
