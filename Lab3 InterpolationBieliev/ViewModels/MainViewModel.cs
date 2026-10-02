using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using InterpolationLab.Models;

namespace InterpolationLab.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private string _h1 = "0,5";
    private string _h2 = "0,05";
    private bool _showReference = true, _showLinear = true, _showHermite = true, _showSupport = true;
    private bool _uneven;
    private string _status = "Готово.";

    public string H1 { get => _h1; set { _h1=value; OnPropertyChanged(); } }
    public string H2 { get => _h2; set { _h2=value; OnPropertyChanged(); } }
    public bool ShowReference { get=>_showReference; set{_showReference=value;OnPropertyChanged();} }
    public bool ShowLinear { get=>_showLinear; set{_showLinear=value;OnPropertyChanged();} }
    public bool ShowHermite { get=>_showHermite; set{_showHermite=value;OnPropertyChanged();} }
    public bool ShowSupport { get=>_showSupport; set{_showSupport=value;OnPropertyChanged();} }
    public bool UnevenPoints { get=>_uneven; set{_uneven=value;OnPropertyChanged();} }
    public string Status { get=>_status; set{_status=value;OnPropertyChanged();} }

    public PointCollection ReferencePoints { get; } = new();
    public PointCollection LinearPoints { get; } = new();
    public PointCollection HermitePoints { get; } = new();
    public PointCollection SupportPoints { get; } = new();
    public PointCollection LinearErrorPoints { get; } = new();
    public PointCollection HermiteErrorPoints { get; } = new();

    public double MaxLinearError { get; private set; }
    public double MaxHermiteError { get; private set; }
    public ICommand BuildCommand { get; }

    public MainViewModel()
    {
        BuildCommand = new RelayCommand(Build);
        Build();
    }

    private void Build()
    {
        try
        {
            double h1 = Positive(H1, "h₁");
            double h2 = Positive(H2, "h₂");
            if (h2 >= h1) throw new ArgumentException("Помилка: h₂ має бути меншим за h₁.");

            List<PointD> a1 = new();
            double end = 2 * Math.PI;

            for (double x=0; x<=end+1e-9; x+=h1)
            {
                double xx=x;
                if (UnevenPoints && a1.Count % 2 == 1) xx += 0.2*h1;
                if (xx > end) xx=end;
                if (a1.Count > 0 && xx <= a1[^1].X) xx=a1[^1].X+0.1*h1;
                if (xx > end) break;
                a1.Add(new PointD(xx, Math.Sin(xx)));
            }

            Interpolation[] methods = { new LinearInterpolation(), new HermiteInterpolation() };

            ReferencePoints.Clear(); LinearPoints.Clear(); HermitePoints.Clear();
            SupportPoints.Clear(); LinearErrorPoints.Clear(); HermiteErrorPoints.Clear();

            for (int i=0; i<1000; i++)
            {
                double x=end*i/999.0;
                ReferencePoints.Add(Screen(x,Math.Sin(x),-1.2,1.2,900,420));
            }

            foreach (PointD p in a1)
                SupportPoints.Add(Screen(p.X,p.Y,-1.2,1.2,900,420));

            double maxL=0, maxH=0;
            for (int i=0; i<1000; i++)
            {
                double x=end*i/999.0;
                double l=methods[0].Calculate(a1,x);
                double h=methods[1].Calculate(a1,x);
                double exact=Math.Sin(x);
                maxL=Math.Max(maxL,Math.Abs(l-exact));
                maxH=Math.Max(maxH,Math.Abs(h-exact));
                LinearPoints.Add(Screen(x,l,-1.2,1.2,900,420));
                HermitePoints.Add(Screen(x,h,-1.2,1.2,900,420));
                LinearErrorPoints.Add(ErrorScreen(x,Math.Abs(l-exact),end));
                HermiteErrorPoints.Add(ErrorScreen(x,Math.Abs(h-exact),end));
            }

            MaxLinearError=maxL; MaxHermiteError=maxH;
            OnPropertyChanged(nameof(MaxLinearError));
            OnPropertyChanged(nameof(MaxHermiteError));
            Status=$"A1: {a1.Count} точок | A2: 1000 точок | " +
                    $"похибка Linear: {maxL:0.000000} | Hermite: {maxH:0.000000}";
        }
        catch(Exception ex)
        {
            Status=ex.Message;
        }
    }

    private static Point Screen(double x,double y,double minY,double maxY,double w,double h)
    {
        const double m=30;
        double sx=m+x/(2*Math.PI)*(w-2*m);
        double sy=h-m-(y-minY)/(maxY-minY)*(h-2*m);
        return new Point(sx,sy);
    }

    private static Point ErrorScreen(double x,double e,double end)
    {
        const double w=900,h=220,m=30,maxE=0.5;
        double sx=m+x/end*(w-2*m);
        double sy=h-m-Math.Min(e,maxE)/maxE*(h-2*m);
        return new Point(sx,sy);
    }

    private static double Positive(string text,string name)
    {
        if(!double.TryParse(text.Trim().Replace('.',','),NumberStyles.Float,
            CultureInfo.GetCultureInfo("uk-UA"),out double v))
            throw new ArgumentException($"{name}: введіть число.");
        if(v<=0) throw new ArgumentException($"{name} має бути > 0.");
        return v;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name=null) =>
        PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
}