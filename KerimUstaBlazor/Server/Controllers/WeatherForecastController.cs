using KerimUstaBlazor.Shared;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using ustakerimhost.Models;

namespace KerimUstaBlazor.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public static string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.150)(PORT = 1521))(CONNECT_DATA=(SERVICE_NAME=TEST)));User Id=ifsapp;Password=ifsapp;";

        [HttpGet]
        [Route("Listele")]
        public List<VeriModel> TestOracleConnection()
        {
            List<VeriModel> veriListesi = new List<VeriModel>();
            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                OracleCommand cmd = new OracleCommand("select * from Yr_Kerimusta", con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    VeriModel model = new VeriModel();
                    model.SIPARIS_NO = dr["SIPARIS_NO"].ToString();
                    model.YAYIN = dr["YAYIN"].ToString();
                    model.REDUKTOR_KOD = dr["REDÜKTÖR_KOD"].ToString();
                    model.REDUKTOR_ACIKLAMA = dr["REDÜKTÖR_ACIKLAMA"].ToString();
                    model.ADET = dr["ADET"].ToString();
                    model.FIYAT = dr["FIYAT"].ToString();
                    model.TOPLAM_SIPARIS_TUTAR = dr["TOPLAM_SIPARIS_TUTAR"].ToString();
                    model.TESLIM_TARIHI = Convert.ToDateTime(dr["TESLIM_TARIHI"]);

                    model.DURUM = dr["DURUM"].ToString();
                    model.GECIKME = dr["GECIKME"].ToString();
                    model.ONAY = dr["ONAY"].ToString();
                    model.SIPARISI_ACAN = dr["SIPARISI_ALAN"].ToString();
                    model.MUSTERI_AD = dr["MÜSTERI_AD"].ToString();
                    veriListesi.Add(model);

                }
                con.Close();

            }
            return veriListesi;
        }

        [HttpGet]
        [Route("ComboDoldur")]
        public List<ComboDoldur> ComboDoldur()
        {
            List<ComboDoldur> model = new List<ComboDoldur>();
            using (OracleConnection con = new OracleConnection(ConnectionString))
            {
                OracleCommand cmd = new OracleCommand("select DISTINCT TRIM(LEADING ' ' FROM x.rowstate) as DURUM from customer_order_line_tab x ORDER BY 1", con);
                con.Open();
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ComboDoldur proc = new ComboDoldur();
                    proc.DURUM = dr["DURUM"].ToString();
                    model.Add(proc);
                }
                con.Close();
            }
            return model;
        }

        [HttpPost]
        [Route("Onayla")]
        public Responen Onayla(Onayla onay)
        {
            Responen respons = new Responen();

            if (onay.value == "Onaysýz")
            {
                using (OracleConnection connection = new OracleConnection(ConnectionString))
                {
                    connection.Open();
                    string selectQuery = $"SELECT ONAY FROM Yr_Ustakerim_Proje2_Tab WHERE SIPARIS_NO = '{onay.sip}' AND YAYIN = '{onay.yay}' AND REDUKTOR_KOD = '{onay.reduk}'";
                    using (OracleCommand selectCommand = new OracleCommand(selectQuery, connection))
                    {
                        OracleDataReader reader = selectCommand.ExecuteReader();
                        if (reader.Read())
                        {
                            // Eðer kayýt varsa ONAY durumunu güncelle
                            string updateQuery = "UPDATE Yr_Ustakerim_Proje2_Tab SET ONAY = 'Onaylý', ONAY_TARIH = :onay_tarih WHERE SIPARIS_NO = :sip AND YAYIN = :yay AND REDUKTOR_KOD = :reduk";

                            using (OracleCommand updateCommand = new OracleCommand(updateQuery, connection))
                            {
                                // Parametreleri ekleyin ve deðerlerini ayarlayýn
                                updateCommand.Parameters.Add(new OracleParameter("onay_tarih", DateTime.Now));
                                updateCommand.Parameters.Add(new OracleParameter("sip", onay.sip));
                                updateCommand.Parameters.Add(new OracleParameter("yay", onay.yay));
                                updateCommand.Parameters.Add(new OracleParameter("reduk", onay.reduk));
                                int rowsAffected = updateCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    //dataGridView1.Rows[selectedRowIndex].Cells["ONAY"].Value = "Onaylý";
                                    //MessageBox.Show("Onay durumu güncellendi.");
                                    respons.Response = "Onaylandý";
                                }
                                else
                                {
                                    respons.Response = "Onaylanmadý";
                                    //MessageBox.Show("Onay durumu güncellenirken hata oluþtu.");
                                }
                            }
                        }
                        else
                        {
                            string insertQuery = "INSERT INTO Yr_Ustakerim_Proje2_Tab (SIPARIS_NO, YAYIN, REDUKTOR_KOD, ONAY, ONAY_TARIH) VALUES (:sip, :yay, :reduk, 'Onaylý', :onay_tarih)";

                            using (OracleCommand insertCommand = new OracleCommand(insertQuery, connection))
                            {
                                // Parametreleri ekleyin ve deðerlerini ayarlayýn
                                insertCommand.Parameters.Add(new OracleParameter("sip", onay.sip));
                                insertCommand.Parameters.Add(new OracleParameter("yay", onay.yay));
                                insertCommand.Parameters.Add(new OracleParameter("reduk", onay.reduk));
                                insertCommand.Parameters.Add(new OracleParameter("onay_tarih", DateTime.Now));
                                int rowsAffected = insertCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    respons.Response = "Onaylandý";
                                }
                                else
                                {
                                    respons.Response = "Onaylanmadý";
                                }
                            }

                        }
                    }
                }
            }
            else if (onay.value == "Onaylý")
            {

                using (OracleConnection connection = new OracleConnection(ConnectionString))
                {
                    connection.Open();
                    // Eðer kayýt varsa ONAY durumunu güncelle
                    string updateQuery = "UPDATE Yr_Ustakerim_Proje2_Tab SET ONAY = 'Onaysýz', ONAY_TARIH = :onay_tarih WHERE SIPARIS_NO = :sip AND YAYIN = :yay AND REDUKTOR_KOD = :reduk";

                    using (OracleCommand updateCommand = new OracleCommand(updateQuery, connection))
                    {
                        // Parametreleri ekleyin ve deðerlerini ayarlayýn
                        updateCommand.Parameters.Add(new OracleParameter("onay_tarih", DateTime.Now));
                        updateCommand.Parameters.Add(new OracleParameter("sip", onay.sip));
                        updateCommand.Parameters.Add(new OracleParameter("yay", onay.yay));
                        updateCommand.Parameters.Add(new OracleParameter("reduk", onay.reduk));
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            respons.Response = "Onaylandý";
                        }
                        else
                        {
                            respons.Response = "Onaylanmadý";
                        }
                    }

                }
            }
            return respons;
        }
    }
}