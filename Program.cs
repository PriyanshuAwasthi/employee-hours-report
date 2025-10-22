using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

class Program
{
    public class EmployeeAttendace
    {
        public string EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string EntryNotes { get; set; }
    }
    static async Task Main()
    {
        string endpoint = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        using HttpClient client = new HttpClient();
        var response = await client.GetStringAsync(endpoint);
        var entries = JsonConvert.DeserializeObject<List<EmployeeAttendace>>(response);

        var times = entries
        .Where(employee => employee.EmployeeName != null)
        .GroupBy(employee => employee.EmployeeName)
        .Select(
            group => new
            {
                Name = group.Key,
                TotalHoursWorked = group.Sum(e => (e.EndTimeUtc - e.StarTimeUtc).TotalHours)
            }
        )
        .OrderByDescending(employee => employee.TotalHoursWorked)
        .ToList();

        string display = @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Employee Work Hours</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 20px; }
                    table { border-collapse: collapse; width: 60%; margin: auto; }
                    th, td { border: 1px solid #999; padding: 8px 12px; text-align: left; }
                    th { background-color: #4CAF50; color: white; }
                    tr.low-hours { background-color: #ffcccc; } /* Red for <100 hrs */
                </style>
            </head>
            <body>
                <h2 style='text-align:center;'>Employee Total Hours Worked</h2>
                <table>
                    <tr><th>Name</th><th>Total Hours</th></tr>";

                    foreach (var emp in times)
                    {
                        string rowClass = emp.TotalHoursWorked < 100 ? " class='low-hours'" : "";
                        display += $"<tr{rowClass}><td>{emp.Name}</td><td>{emp.TotalHoursWorked:F2}</td></tr>";
                    }

        display += @"
                </table>
            </body>
            </html>";


        string chartFilePath = Path.Combine(Directory.GetCurrentDirectory(), "EmployeeHoursPieChart.png");

        var plotModel = new PlotModel { Title = "Employee Work Hours Pie Chart" };
        plotModel.Background = OxyColors.White;
        var pieSeries = new PieSeries
        {
            StrokeThickness = 1.0,
            InsideLabelPosition = 0.5,
            AngleSpan = 360,
            StartAngle = 0
        };

        foreach (var emp in times)
        {
            pieSeries.Slices.Add(new PieSlice(emp.Name, emp.TotalHoursWorked));
        }

        plotModel.Series.Add(pieSeries);

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "EmployeeHours.html");

        var pngExporter = new PngExporter { Width = 600, Height = 400 };
        using (var stream = File.Create(chartFilePath))
        {
            pngExporter.Export(plotModel, stream);
        }

        await File.WriteAllTextAsync(filePath, display);
    }
}