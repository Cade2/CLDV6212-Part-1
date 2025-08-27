using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using ST10443998_CLDV6212_POE.Services;

namespace ST10443998_CLDV6212_POE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            var conn = builder.Configuration.GetConnectionString("AzureStorage")!;
            var container = builder.Configuration["AzureStorage:BlobContainer"]!;
            var share = builder.Configuration["AzureStorage:FileShare"]!;
            var queue = builder.Configuration["AzureStorage:QueueName"]!;
            var custTable = builder.Configuration["AzureStorage:CustomersTable"] ?? "Customers";
            var prodTable = builder.Configuration["AzureStorage:ProductsTable"] ?? "Products";

            builder.Services.AddSingleton(new BlobContainerClient(conn, container));
            builder.Services.AddSingleton(new ShareClient(conn, share));
            builder.Services.AddSingleton(new QueueClient(conn, queue));

            builder.Services.AddSingleton<CustomerTableService>(_ =>
                new CustomerTableService(new TableClient(conn, custTable)));
            builder.Services.AddSingleton<ProductTableService>(_ =>
                new ProductTableService(new TableClient(conn, prodTable)));

            builder.Services.AddSingleton<BlobImageService>();
            builder.Services.AddSingleton<FileContractService>();
            builder.Services.AddSingleton<OrderQueueService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
            app.Run();
        }
    }
}
