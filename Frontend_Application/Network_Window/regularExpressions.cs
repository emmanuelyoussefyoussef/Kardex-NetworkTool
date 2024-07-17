using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Network_Window;

namespace Network_Window
{
    public class regularExpressions
    {
        private readonly string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        private String impIP;
        private String impMask;
        private String impGateway;
        private String impIndex;
        private Boolean isValid = false;
        private String output;

        private MainWindow mainWindow;

        public regularExpressions(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        public regularExpressions() { }
        public void patternValiduation() {
            if (!Regex.IsMatch(getImpIP(), pattern))
            {
                setOutput("IP Adresse ist ungültig");
            }
            else if (!Regex.IsMatch(getImpMask(), pattern))
            {
                setOutput("Subnetzmaske ist ungültig");
            }
            else if (!Regex.IsMatch(getImpGateway(), pattern))
            {
                setOutput("Gateway ist ungültig");
            }
            else if (mainWindow.counter == 5)
            {
                setOutput("Maximale Anzahl an Routen erreicht, bitte löschen sie Routen.");
            }
            else setIsValid(true);
        }


        public void setImpIP(String impIP)
        {
            this.impIP = impIP;
        }
        public String getImpIP()
        {
            return impIP;
        }
        public void setImpMask(String impMask)
        {
            this.impMask = impMask;
        }
        public String getImpMask()
        {
            return impMask;
        }
        public void setImpGateway(String impGateway)
        {
            this.impGateway = impGateway;
        }
        public String getImpGateway()
        {
            return impGateway;
        }
        public void setImpIndex(String impIndex)
        {
            this.impIndex = impIndex;
        }
        public String getImpIndex()
        {
            return impIndex;
        }
        public void setIsValid(Boolean isValid)
        {
            this.isValid = isValid;
        }
        public Boolean getIsValid()
        {
            return isValid;
        }
        public void setOutput(String output)
        {
            this.output = output;
        }
        public String getOutput()
        {
            return output;
        }
        
    }
}
