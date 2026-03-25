using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;

namespace BenimFinansim
{
    public class KurServisi
    {
        // 1. DÖVİZ RADARI (Merkez Bankası - TCMB)
        public static async Task<double> KuruGetir(string paraBirimiKod)
        {
            try
            {
                string url = "https://www.tcmb.gov.tr/kurlar/today.xml";
                using (HttpClient client = new HttpClient())
                {
                    string xmlVerisi = await client.GetStringAsync(url);
                    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(xmlVerisi);

                    System.Xml.XmlNode? node = xmlDoc.SelectSingleNode($"Tarih_Date/Currency[@Kod='{paraBirimiKod}']/ForexSelling");
                    if (node != null && !string.IsNullOrEmpty(node.InnerText))
                    {
                        return double.Parse(node.InnerText, CultureInfo.InvariantCulture);
                    }
                }
            }
            catch { return 0; }
            return 0; 
        }

        // 2. SERBEST PİYASA ALTIN/GÜMÜŞ RADARI (Truncgil Finans API)
        public static async Task<double> MadenGetir(string madenKodu)
        {
            try
            {
                // madenKodu: "gram-altin" veya "gumus"
                string url = "https://finans.truncgil.com/today.json";
                
                using (HttpClient client = new HttpClient())
                {
                    string jsonVerisi = await client.GetStringAsync(url);
                    
                    using (JsonDocument doc = JsonDocument.Parse(jsonVerisi))
                    {
                        JsonElement root = doc.RootElement;
                        
                        if (root.TryGetProperty(madenKodu, out JsonElement madenNode))
                        {
                            // Truncgil API fiyatı "2.450,50" veya "96,60" şeklinde Türk formatında verir
                            string satisFiyatiStr = madenNode.GetProperty("Satış").GetString() ?? "0";
                            
                            // Binlik ayracı (nokta) silip, ondalık ayracı (virgül) noktaya çeviriyoruz ki C# anlasın
                            satisFiyatiStr = satisFiyatiStr.Replace(".", "").Replace(",", ".");
                            
                            return double.Parse(satisFiyatiStr, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            catch 
            { 
                // İnternet koparsa güncel kura yakın bir güvenlik ağı (Çökmesin diye)
                if (madenKodu == "gram-altin") return 2450.00;
                if (madenKodu == "gumus") return 96.50; 
            }
            return 0;
        }
    }
}