

using LoadDWVentas.Data.Context;
using LoadDWVentas.Data.Entities.DwVentas;
using LoadDWVentas.Data.Interfaces;
using LoadDWVentas.Data.Result;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LoadDWVentas.Data.Services
{
    public class DataServiceDwVentas : IDataServiceDwVentas
    {
        private readonly NorwindContext _norwindContext;
        private readonly DbSalesContext _salesContext;

        public DataServiceDwVentas(NorwindContext norwindContext,
                                   DbSalesContext salesContext)
        {
            _norwindContext = norwindContext;
            _salesContext = salesContext;
        }

        public async Task<OperactionResult> LoadDHW()
        {
            OperactionResult result = new OperactionResult();
            try
            {

                await LoadDimEmployee();
                await LoadDimProductCategory();
                await LoadDimCustomers();
                await LoadFactSales();
                await LoadFactCustomerServed();


            }
            catch (Exception ex)
            {

                result.Success = false;
                result.Message = $"Error cargando el DWH Ventas. {ex.Message}";
            }

            return result;
        }

        private async Task<OperactionResult> LoadDimEmployee()
        {
            OperactionResult result = new OperactionResult();

            try
            {
                //Obtener los empleados de la base de datos de norwind.
                var employees = await _norwindContext.Employees.AsNoTracking().Select(emp => new DimEmployee()
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName = string.Concat(emp.FirstName, " ", emp.LastName)
                }).ToListAsync();

                int[] employeeIds = employees.Select(emp => emp.EmployeeId).ToArray();

                // Carga la dimension de empleados.
                await _salesContext.DimEmployees.Where(cd => employeeIds.Contains(cd.EmployeeId))
                                                .AsNoTracking()
                                                .ExecuteDeleteAsync();

                await _salesContext.DimEmployees.AddRangeAsync(employees);

                await _salesContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                result.Success = false;
                result.Message = $"Error cargando la dimension de empleado {ex.Message}";
            }


            return result;
        }

        private async Task<OperactionResult> LoadDimProductCategory()
        {
            OperactionResult result = new OperactionResult();
            try
            {
                // Obtener las products categories de norwind //

                var productCategories = await (from product in _norwindContext.Products
                                               join category in _norwindContext.Categories on product.CategoryId equals category.CategoryId
                                               select new DimProductCategory()
                                               {
                                                   CategoryId = category.CategoryId,
                                                   ProductName = product.ProductName,
                                                   CategoryName = category.CategoryName,
                                                   ProductId = product.ProductId
                                               }).AsNoTracking().ToListAsync();


                // Carga la dimension de Products Categories.

                int[] productsIds = productCategories.Select(c => c.ProductId).ToArray();


                await _salesContext.DimProductCategories.Where(c => productsIds.Contains(c.ProductId))
                                                        .AsNoTracking()
                                                        .ExecuteDeleteAsync();

                await _salesContext.DimProductCategories.AddRangeAsync(productCategories);

                await _salesContext.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error cargando la dimension de producto y categoria. {ex.Message}";
            }
            return result;
        }

        private async Task<OperactionResult> LoadDimCustomers()
        {
            OperactionResult operaction = new OperactionResult() { Success = false };


            try
            {
                // Obtener clientes de norwind

                var customers = await _norwindContext.Customers.Select(cust => new DimCustomer()
                {
                    CustomerId = cust.CustomerId,
                    CustomerName = cust.CompanyName

                }).AsNoTracking()
                  .ToListAsync();

                // Carga dimension de cliente.

                string[] customersIds = customers.Select(cust => cust.CustomerId).ToArray();

                await _salesContext.DimCustomers.Where(cust => customersIds.Contains(cust.CustomerId))
                                          .AsNoTracking()
                                          .ExecuteDeleteAsync();

                await _salesContext.DimCustomers.AddRangeAsync(customers);
                await _salesContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                operaction.Success = false;
                operaction.Message = $"Error: {ex.Message} cargando la dimension de clientes.";
            }
            return operaction;
        }

        private async Task<OperactionResult> LoadFactSales()
        {
            OperactionResult result = new OperactionResult();

            try
            {
                var ventas = await _norwindContext.Vwventas.AsNoTracking().ToListAsync();


                int[] ordersId = await _salesContext.FactOrders.Select(cd => cd.OrderNumber).ToArrayAsync();

                if (ordersId.Any())
                {
                    await _salesContext.FactOrders.Where(cd => ordersId.Contains(cd.OrderNumber))
                                                  .AsNoTracking()
                                                  .ExecuteDeleteAsync();
                }

                foreach (var venta in ventas)
                {
                    var customer = await _salesContext.DimCustomers.SingleOrDefaultAsync(cust => cust.CustomerId == venta.CustomerId);
                    var employee = await _salesContext.DimEmployees.SingleOrDefaultAsync(emp => emp.EmployeeId == venta.EmployeeId);
                    var shipper = await _salesContext.DimShippers.SingleOrDefaultAsync(ship => ship.ShipperId == venta.ShipperId);
                    var product = await _salesContext.DimProductCategories.SingleOrDefaultAsync(pro => pro.ProductId == venta.ProductId);

                    FactOrder factOrder = new FactOrder()
                    {
                        CantidadVentas = venta.Cantidad.Value,
                        Country = venta.Country,
                        CustomerKey = customer.CustomerKey,
                        EmployeeKey = employee.EmployeeKey,
                        DateKey = venta.DateKey.Value,
                        ProductKey = product.ProductKey,
                        Shipper = shipper.ShipperKey,
                        TotalVentas = Convert.ToDecimal(venta.TotalVentas)
                    };

                    await _salesContext.FactOrders.AddAsync(factOrder);

                    await _salesContext.SaveChangesAsync();
                }



                result.Success = true;
            }
            catch (Exception ex)
            {

                result.Success = false;
                result.Message = $"Error cargando el fact de ventas {ex.Message} ";
            }

            return result;
        }

        private async Task<OperactionResult> LoadFactCustomerServed()
        {
            OperactionResult result = new OperactionResult() { Success = true };

            try
            {
                var customerServeds = await _norwindContext.VwServedCustomers.AsNoTracking().ToListAsync();

                int[] customerIds = _salesContext.FactClienteAtendidos.Select(cli => cli.ClienteAtendidoId).ToArray();

                //Limpiamos la tabla de facts //

                if (customerIds.Any())
                {
                    await _salesContext.FactClienteAtendidos.Where(fact => customerIds.Contains(fact.ClienteAtendidoId))
                                                            .AsNoTracking()
                                                            .ExecuteDeleteAsync();
                }

                //Carga el fact de clientes atendidos. //
                foreach (var customer in customerServeds)
                {
                    var employee = await _salesContext.DimEmployees
                                                      .SingleOrDefaultAsync(emp => emp.EmployeeId ==
                                                                               customer.EmployeeId);


                    FactClienteAtendido factClienteAtendido = new FactClienteAtendido()
                    {
                        EmployeeKey = employee.EmployeeKey,
                        TotalClientes = customer.TotalCustomersServed
                    };


                    await _salesContext.FactClienteAtendidos.AddAsync(factClienteAtendido);

                    await _salesContext.SaveChangesAsync();
                }

                result.Success = true;

            }
            catch (Exception ex)
            {

                result.Success = false;
                result.Message = $"Error cargando el fact de clientes atendidos {ex.Message} ";
            }
            return result;
        }
    }
}