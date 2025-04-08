/// <summary>
/// EmployeeController provides an HTTP GET endpoint for querying employee records
/// using a parameterized SQL Server stored procedure (usp_SearchEmployees).
/// 
/// The controller supports:
/// - Filtering by department, active status, and keyword search
/// - Paging using page number and page size
/// - Selecting output format (JSON or XML)
/// 
/// It uses Dapper for lightweight database access and returns the raw result
/// from SQL Server, formatted by the stored procedure.
/// 
/// This controller demonstrates how to pass user input to a stored procedure,
/// handle nullable parameters, and return raw SQL output directly to the client.
/// </summary>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using System.Threading.Tasks;

namespace DemoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEmployees(
            int? departmentId,
            bool? isActive,
            string search = null,
            int page = 1,
            int pageSize = 20,
            string outputFormat = "JSON")
        {
            // Get DB connection string from config
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);

            // Prepare parameters for stored procedure
            var parameters = new DynamicParameters();
            parameters.Add("@DepartmentID", departmentId, DbType.Int32);
            parameters.Add("@IsActive", isActive, DbType.Boolean);
            parameters.Add("@Search", search, DbType.String);
            parameters.Add("@Page", page, DbType.Int32);
            parameters.Add("@PageSize", pageSize, DbType.Int32);
            parameters.Add("@OutputFormat", outputFormat, DbType.String);

            // Call the stored procedure
            var result = await connection.QueryFirstOrDefaultAsync<string>(
                "usp_SearchEmployees",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // Return the raw string with proper content-type
            if (outputFormat.ToUpper() == "XML")
                return Content(result ?? "<Employees></Employees>", "application/xml");
            else
                return Content(result ?? "[]", "application/json");
        }
    }
}
