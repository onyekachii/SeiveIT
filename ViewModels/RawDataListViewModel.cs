﻿using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Maui.Skia;
using OxyPlot.Series;
using SeiveIT.Entities;
using SeiveIT.Models;
using SeiveIT.Services.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SeiveIT.ViewModels
{
    public partial class RawDataListViewModel : ObservableObject
    {
        IServiceManager _serviceManager = App.serviceManager;
        public long Pid { get; set; }
        public long Oid { get; set; }
        public ObservableCollection<RawDataViewModel> Rows { get; set; }
        [ObservableProperty]
        double _totalWeight;
        [ObservableProperty]
        double _totalInd;
        [ObservableProperty]
        PlotModel _plotModeler;

        public RawDataListViewModel(long pid, long oid)
        {
            Pid = pid;
            Oid = oid;
            Load();
        }

        private PlotModel CreatePlotModel()
        {
            var plotModel = new PlotModel { Title = "Semi-Log Plot" };
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Linear X-Axis",
                Minimum = 0,
                Maximum = 100,
                MinorGridlineStyle = LineStyle.LongDashDot,
                MajorGridlineStyle = LineStyle.Solid,
            });
            plotModel.Axes.Add(new LogarithmicAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Log Y-Axis",
                Base = 10, // Logarithm base (10 for log10, 2 for log2, etc.)
                Minimum = 1,  // Minimum value (10^0 = 1)
                Maximum = 10000, // Maximum value (10^4 = 10000)
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });
            var series = new LineSeries();
            foreach (var row in Rows)
            {
                series.Points.Add(new DataPoint(row.CummWeight, row.IndWeight));
            }
            plotModel.Series.Add(series);
            // Add your axes and series
            return plotModel;
        }

        void Load()
        {
            List<SeiveData> data = Task.Run(async ()=> await _serviceManager.RawDataService.Get(Pid, Oid)).Result;
            if (data.Count > 0)
            {
                Rows = new ObservableCollection<RawDataViewModel>(data.OrderBy(d => d.PhiScale).Select(r => new RawDataViewModel
                {
                    PhiScale = r.PhiScale,
                    Weight = r.Weight.ToString(),
                    Id = r.Id
                }));
                Run();
                PlotModeler = CreatePlotModel();
            }
            else
            {
                Rows = new ObservableCollection<RawDataViewModel>(RawData.GetRows()
                .Select(r => new RawDataViewModel
                {
                    CummWeight = r.CummWeight,
                    IndWeight = r.IndWeight,
                    PhiScale = r.PhiScale,
                    RowNumber = r.RolNum,
                    Weight = r.Weight.ToString(),
                    CummPassing = r.CummPassing
                }));
            }
        }

        [RelayCommand]
        void Run()
        {
            TotalWeight = Math.Round(Rows.Sum(r => float.Parse(r.Weight)), 1);
            double totalInd = 0;
            double prevRow = 0;
            foreach (var row in Rows)
            {
                row.IndWeight = Math.Round( (100 * float.Parse(row.Weight)) / TotalWeight, 3);
                row.CummWeight = 0;
                row.CummWeight = Math.Round(prevRow + row.IndWeight, 3);
                prevRow = row.CummWeight;
                totalInd += row.IndWeight;
            }
            TotalInd = Math.Round(totalInd);  
            foreach(var row in Rows)
            {
                row.CummPassing = Math.Round(TotalInd - row.IndWeight, 3);
            }
        }

        [RelayCommand]
        async Task Save()
        {
            try
            {
                var data = Rows.Select(r => new SeiveData
                {
                    Id = r.Id,
                    PhiScale = r.PhiScale,
                    Weight = float.Parse(r.Weight),
                    OutcropId = Oid,
                    ProjectId = Pid
                }).ToList();
                await _serviceManager.RawDataService.UpsertSeiveData(data);                                           

                await Toast.Make("Project saved").Show();
                //await Shell.Current.GoToAsync($"project?id={proj.Id}");
            }
            catch (SQLite.SQLiteException e)
            {
                var a = e.GetType();
                string errMsg = e.Message.Contains("UNIQUE constraint failed") ? "Title already exists" : "Unable to Save data, send us a mail";
                await Toast.Make(errMsg).Show();
            }
            catch (Exception e)
            {
                await Toast.Make("An error has occured, send us a mail").Show();
            }
        }
    }
}
