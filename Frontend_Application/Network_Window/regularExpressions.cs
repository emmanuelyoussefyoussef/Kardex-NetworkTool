using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Network_Window;

namespace Network_Window
{
    //public class RegularExpressions
    //{
    //    private readonly string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

    //    public string ImpIp { get; set; }
    //    public string ImpMask { get; set; }
    //    public string ImpGateway { get; set;}
    //    public string ImpIndex { get; set;}
    //    private Boolean IsValid = false;
    //    public string Output { get; set; }
    //    private MainWindow mainWindow;

    //    //private MainWindow mainWindow;

    //    //public RegularExpressions(MainWindow mainWindow)
    //    //{
    //    //    this.mainWindow = mainWindow;
    //    //}
    //    public RegularExpressions(MainWindow mainWindow) {
    //        this.mainWindow = mainWindow;
    //    }

    //    public void patternValiduation() {
    //        if (!Regex.IsMatch(ImpIp, pattern))
    //        {
    //            Output="IP Adresse ist ungültig";
    //        }
    //        else if (!Regex.IsMatch(ImpMask, pattern))
    //        {
    //            Output = "Subnetzmaske ist ungültig";
    //        }
    //        else if (!Regex.IsMatch(ImpGateway, pattern))
    //        {
    //            Output = "Gateway ist ungültig";
    //        }
    //        else if (mainWindow.counter == 5)
    //        {
    //            Output = "Maximale Anzahl an Routen erreicht, bitte löschen sie Routen.";
    //        }
    //        else setIsValid(true);
    //    }
    //    public void setIsValid(Boolean isValid)
    //    {
    //        this.isValid = isValid;
    //    }
    //    public Boolean getIsValid()
    //    {
    //        return isValid;
    //    }

        
    //}
}
