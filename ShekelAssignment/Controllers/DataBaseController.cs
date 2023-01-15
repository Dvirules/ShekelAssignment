using System.Linq;
using System.Web.Http;
using System.Data.SqlClient;

namespace ShekelAssignment.Controllers
{
    [RoutePrefix("api/sql")]
    public class DataBaseController : ApiController
    {
        private readonly string _connectionString = "ShekelTest";

        // Fetch "groupCode" and "groupName" values from the "Groups" table
        // and "customerld" and "name" values from the "Customers" table associated with each group
        [HttpGet]
        [Route("groupsandcustomers")]
        public IHttpActionResult GetGroupsAndCustomers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT groupCode, groupName, customerld, name " +
                            "FROM FactoriesToCustomer fc " +
                            "INNER JOIN Groups g ON g.groupCode = fc.groupCode " +
                            "INNER JOIN Customers c ON c.customerld = fc.customerld";

                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    var groupsAndCustomers = new List<GroupAndCustomer>();

                    while (reader.Read())
                    {
                        var groupAndCustomer = new GroupAndCustomer
                        {
                            GroupCode = reader["groupCode"] == null ? "None" : reader["groupCode"].ToString(),
                            GroupName = reader["groupName"] == null ? "None" : reader["groupName"].ToString(),
                            CustomerId = reader["customerld"] == null ? "None" : reader["customerld"].ToString(),
                            CustomerName = reader["name"] == null ? "None" : reader["name"].ToString()
                        };

                        groupsAndCustomers.Add(groupAndCustomer);
                    }

                    return Ok(groupsAndCustomers);
                }
            }
        }

        //Add a new Customer
        [HttpPost]
        [Route("addcustomer")]
        public IHttpActionResult AddCustomer(AddCustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Insert new customer into the "Customers" table
                var query = "INSERT INTO Customers (customerld, name, address, phone) " +
                            "VALUES (@customerId, @name, @address, @phone)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@customerId", request.CustomerId);
                    command.Parameters.AddWithValue("@name", request.Name);
                    command.Parameters.AddWithValue("@address", request.Address);
                    command.Parameters.AddWithValue("@phone", request.Phone);

                    command.ExecuteNonQuery();
                }

                // Associate the new customer with their group and factory
                query = "INSERT INTO FactoriesToCustomer (customerld, groupCode, factoryCode) " +
                        "VALUES (@customerId, @groupCode, @factoryCode)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@customerId", request.CustomerId);
                    command.Parameters.AddWithValue("@groupCode", request.GroupCode);
                    command.Parameters.AddWithValue("@factoryCode", request.FactoryCode);
                    command.ExecuteNonQuery();
                }

                return Ok();
            }
        }
    }
}

