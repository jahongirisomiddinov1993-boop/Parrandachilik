using System;
using System.Windows;
using System.Data.SqlClient;

namespace Parrandachilik
{
    public partial class AddTuxumWindow : Window
    {
        string connectionString = @"Data Source=.;Initial Catalog=ParrandaDB;Integrated Security=True;";

        public AddTuxumWindow()
        {
            InitializeComponent();
        }

        private void btnSaqlash_Click(object sender, RoutedEventArgs e)
        {
            // Sana, SexNomi va XODIM to'g'ri o'qildi (XATO TUZATILDI)
            string sana = dpSana.SelectedDate.HasValue ? dpSana.SelectedDate.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
            string sexNomi = txtSexNomi.Text;
            string xodim = txtXodim.Text; // MANA SHU QATOR QO'SHILDI

            double w16 = double.TryParse(txt16.Text, out double r16) ? r16 : 0;
            double w17 = double.TryParse(txt17.Text, out double r17) ? r17 : 0;
            double w18 = double.TryParse(txt18.Text, out double r18) ? r18 : 0;
            double w19 = double.TryParse(txt19.Text, out double r19) ? r19 : 0;
            double w20 = double.TryParse(txt20.Text, out double r20) ? r20 : 0;
            double w25 = double.TryParse(txt25.Text, out double r25) ? r25 : 0;
            double siniq = double.TryParse(txtSiniq.Text, out double rs) ? rs : 0;
            double paket = double.TryParse(txtPaket.Text, out double rp) ? rp : 0;

            SaveToDatabase(sana, sexNomi, xodim, w16, w17, w18, w19, w20, w25, siniq, paket);
        }

        private void SaveToDatabase(string sana, string sexNomi, string xodim, double w16, double w17, double w18, double w19, double w20, double w25, double siniq, double paket)
        {
            string query = @"INSERT INTO Tuxumlar 
                             (Sana, SexNomi, HodimFISH, W1_6, W1_7, W1_8, W1_9, W2_0, W2_5, Siniq, Paket) 
                             VALUES 
                             (@Sana, @SexNomi, @HodimFISH, @W1_6, @W1_7, @W1_8, @W1_9, @W2_0, @W2_5, @Siniq, @Paket)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Sana", sana);
                        cmd.Parameters.AddWithValue("@SexNomi", sexNomi);
                        cmd.Parameters.AddWithValue("@HodimFISH", xodim);
                        cmd.Parameters.AddWithValue("@W1_6", w16);
                        cmd.Parameters.AddWithValue("@W1_7", w17);
                        cmd.Parameters.AddWithValue("@W1_8", w18);
                        cmd.Parameters.AddWithValue("@W1_9", w19);
                        cmd.Parameters.AddWithValue("@W2_0", w20);
                        cmd.Parameters.AddWithValue("@W2_5", w25);
                        cmd.Parameters.AddWithValue("@Siniq", siniq);
                        cmd.Parameters.AddWithValue("@Paket", paket);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Muvaffaqiyatli saqlandi!", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xatolik yuz berdi: " + ex.Message, "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}