using System.Data;
using MySqlConnector;

class Ambulance
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsAvailable { get; set; }
}

class AmbulanceBooking
{
    public int Id { get; set; }
    public int AmbulanceId { get; set; }
    public DateTime BookingTime { get; set; }
    public string User { get; set; }
}

class AmbulanceBookingSystem
{
    private MySqlConnection connection;
    private string connectionString;

    public AmbulanceBookingSystem(string server, string database, string username, string password)
    {
        connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
        connection = new MySqlConnection(connectionString);
    }

    public void OpenConnection()
    {
        try
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening database connection: " + ex.Message);
        }
    }

    public void CloseConnection()
    {
        try
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error closing database connection: " + ex.Message);
        }
    }

    public void AddAmbulance(Ambulance ambulance)
    {
        try
        {
            OpenConnection();

            string query = "INSERT INTO ambulances (Name, PhoneNumber, IsAvailable) " +
                           "VALUES (@Name, @PhoneNumber, @IsAvailable)";

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", ambulance.Name);
            command.Parameters.AddWithValue("@PhoneNumber", ambulance.PhoneNumber);
            command.Parameters.AddWithValue("@IsAvailable", ambulance.IsAvailable);

            command.ExecuteNonQuery();

            CloseConnection();
            Console.WriteLine("Ambulance added successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error adding ambulance: " + ex.Message);
        }
    }

    public List<Ambulance> GetAmbulances()
    {
        List<Ambulance> ambulances = new List<Ambulance>();

        try
        {
            OpenConnection();

            string query = "SELECT * FROM ambulances";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Ambulance ambulance = new Ambulance
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                };

                ambulances.Add(ambulance);
            }

            reader.Close();
            CloseConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting ambulances: " + ex.Message);
        }

        return ambulances;
    }

    public void UpdateAmbulanceAvailability(Ambulance ambulance)
    {
        try
        {
            OpenConnection();

            string query = "UPDATE ambulances SET IsAvailable = @IsAvailable WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@IsAvailable", ambulance.IsAvailable);
            command.Parameters.AddWithValue("@Id", ambulance.Id);

            command.ExecuteNonQuery();

            CloseConnection();
            Console.WriteLine("Ambulance availability updated successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating ambulance availability: " + ex.Message);
        }
    }

    public void DeleteAmbulance(int ambulanceId)
    {
        try
        {
            OpenConnection();

            string query = "DELETE FROM ambulances WHERE Id = @Id";

            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", ambulanceId);

            command.ExecuteNonQuery();

            CloseConnection();
            Console.WriteLine("Ambulance deleted successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting ambulance: " + ex.Message);
        }
    }

    public void BookAmbulance(int ambulanceId, string user)
    {

        try
        {
            OpenConnection();

            string query = "SELECT * FROM ambulances WHERE Id = @Id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", ambulanceId);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                bool isAvailable = Convert.ToBoolean(reader["IsAvailable"]);

                reader.Close();

                if (isAvailable)
                {
                    query = "UPDATE ambulances SET IsAvailable = false WHERE Id = @Id";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", ambulanceId);
                    command.ExecuteNonQuery();

                    query = "INSERT INTO bookings (AmbulanceId, BookingTime, User) VALUES (@AmbulanceId, @BookingTime, @User)";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@AmbulanceId", ambulanceId);
                    command.Parameters.AddWithValue("@BookingTime", DateTime.Now);
                    command.Parameters.AddWithValue("@User", user);
                    command.ExecuteNonQuery();

                    Console.WriteLine("Ambulance booked successfully!");
                }
                else
                {
                    Console.WriteLine("Ambulance is not available for booking!");
                }
            }
            else
            {
                reader.Close();
                Console.WriteLine("Ambulance not found!");
            }

            CloseConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error booking ambulance: " + ex.Message);
        }
    }

    public void ViewBookingHistory()
    {
        try
        {
            OpenConnection();

            string query = "SELECT * FROM bookings";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();

            Console.WriteLine("Booking History:");
            Console.WriteLine("ID\tAmbulance ID\tBooking Time\tUser");

            while (reader.Read())
            {
                Console.WriteLine($"{reader["Id"]}\t{reader["AmbulanceId"]}\t{reader["BookingTime"]}\t{reader["User"]}");
            }

            reader.Close();
            CloseConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error viewing booking history: " + ex.Message);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter MySQL server: ");
        string server = Console.ReadLine();

        Console.Write("Enter database name: ");
        string database = Console.ReadLine();

        Console.Write("Enter username: ");
        string username = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        AmbulanceBookingSystem bookingSystem = new AmbulanceBookingSystem(server, database, username, password);
        string currentUser = string.Empty;

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Ambulance Booking System");
            Console.WriteLine("1. Add Ambulance");
            Console.WriteLine("2. View Ambulances");
            Console.WriteLine("3. Update Ambulance Availability");
            Console.WriteLine("4. Delete Ambulance");
            Console.WriteLine("5. Book Ambulance");
            Console.WriteLine("6. View Booking History");
            Console.WriteLine("7. Switch User");
            Console.WriteLine("8. Exit");
            Console.Write("Enter your choice (1-8): ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Ambulance ambulance = new Ambulance();

                    Console.WriteLine("Enter ambulance details:");
                    Console.Write("Name: ");
                    ambulance.Name = Console.ReadLine();

                    Console.Write("Phone number: ");
                    ambulance.PhoneNumber = Console.ReadLine();

                    ambulance.IsAvailable = true;

                    bookingSystem.AddAmbulance(ambulance);
                    break;
                case 2:
                    List<Ambulance> ambulances = bookingSystem.GetAmbulances();

                    Console.WriteLine("Ambulance List:");
                    Console.WriteLine("ID\tName\tPhone Number\tAvailability");

                    foreach (Ambulance amb in ambulances)
                    {
                        Console.WriteLine($"{amb.Id}\t{amb.Name}\t{amb.PhoneNumber}\t{amb.IsAvailable}");
                    }
                    break;
                case 3:
                    Console.Write("Enter the ID of the ambulance to update: ");
                    int ambulanceId = Convert.ToInt32(Console.ReadLine());

                    ambulances = bookingSystem.GetAmbulances();
                    Ambulance selectedAmbulance = ambulances.Find(a => a.Id == ambulanceId);

                    if (selectedAmbulance != null)
                    {
                        Console.Write("Is the ambulance available? (Y/N): ");
                        string input = Console.ReadLine();

                        if (input.ToUpper() == "Y")
                            selectedAmbulance.IsAvailable = true;
                        else
                            selectedAmbulance.IsAvailable = false;

                        bookingSystem.UpdateAmbulanceAvailability(selectedAmbulance);
                    }
                    else
                    {
                        Console.WriteLine("Ambulance not found!");
                    }
                    break;
                case 4:
                    Console.Write("Enter the ID of the ambulance to delete: ");
                    ambulanceId = Convert.ToInt32(Console.ReadLine());

                    ambulances = bookingSystem.GetAmbulances();
                    selectedAmbulance = ambulances.Find(a => a.Id == ambulanceId);

                    if (selectedAmbulance != null)
                    {
                        bookingSystem.DeleteAmbulance(ambulanceId);
                    }
                    else
                    {
                        Console.WriteLine("Ambulance not found!");
                    }
                    break;
                case 5:
                    if (string.IsNullOrEmpty(currentUser))
                    {
                        Console.WriteLine("Please switch to a user account to book an ambulance.");
                    }
                    else
                    {
                        Console.Write("Enter the ID of the ambulance to book: ");
                        ambulanceId = Convert.ToInt32(Console.ReadLine());

                        bookingSystem.BookAmbulance(ambulanceId, currentUser);
                    }
                    break;
                case 6:
                    if (string.IsNullOrEmpty(currentUser))
                    {
                        Console.WriteLine("Please switch to an admin account to view the booking history.");
                    }
                    else
                    {
                        bookingSystem.ViewBookingHistory();
                    }
                    break;
                case 7:
                    Console.Write("Enter username (admin/user): ");
                    currentUser = Console.ReadLine();
                    break;
                case 8:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice! Please enter a valid option.");
                    break;
            }
        }
    }
}
