using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Network_Window;

namespace Network_Window
{
    public class RegularExpressions
    {
        private readonly string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        public string ImpIp { get; set; }
        public string ImpMask { get; set; }
        public string ImpGateway { get; set; }
        public string ImpIndex { get; set; }
        public string Output { get; set; }
        public bool IsValid = false;
        //private MainWindow mainWindow;

        //public RegularExpressions(MainWindow mainWindow)
        //{
        //    this.mainWindow = mainWindow;
        //}
        public RegularExpressions()
        {

        }
        public bool PatternValiduation()
        {
            IsValid = true;
            if (!Regex.IsMatch(ImpIp, pattern))
            {
                IsValid = false;
                Output = "IP Adresse ist ungültig";
            }
            else if (!Regex.IsMatch(ImpMask, pattern))
            {
                IsValid = false;
                Output = "Subnetzmaske ist ungültig";
            }
            else if (!Regex.IsMatch(ImpGateway, pattern))
            {
                IsValid = false;
                Output = "Gateway ist ungültig";
            }
            else if (string.IsNullOrEmpty(ImpIndex))
            {
                IsValid = false;
                Output = "Index ist ungültig";
            }
            return IsValid;
        }
    }
}
