using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO.Ports;

namespace AvensAkbilDeneme
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Panel[] panels;
        string[] ports = SerialPort.GetPortNames();
        int bulunanPanel;
        string konum1;

        private void Form1_Load(object sender, EventArgs e)
        {
            konum1 = konum.konum;
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }
            panels = new Panel[] {panelKartEkle, panelPortBagla, panelParaYukle, panelOkuma};
            PanelAc(1);
        }

        Class1 konum = new Class1();


        SqlConnection baglan;
        SqlCommand komut;

        private void button4_Click(object sender, EventArgs e)
        {
            baglan = new SqlConnection(konum1);

            if (textBox1.Text.Replace(" ", "") == "" || textBox2.Text.Replace(" ", "") == "" || comboBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Bu alanlor boş bırakılamaz", "HATA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (comboBox2.Text.Length > 10)
            {
                MessageBox.Show("Lütfen tarife seçim bölü");
            }
            else
            {
                baglan.Open();
                komut = new SqlCommand("delete KartKayit where KartID = '" + textBox3.Text + "'", baglan);
                komut.ExecuteNonQuery();
                komut = new SqlCommand("insert into KartKayit (Isim, Soyisim, KartID, Para, Tarife) values ('" + textBox1.Text + "', '" + textBox2.Text + "', '" + textBox3.Text + "', '" + 0.ToString() + "', '" + comboBox2.Text + "')", baglan);
                komut.ExecuteNonQuery();
                baglan.Close();
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                comboBox2.ResetText();
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button5.Enabled = true;
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = 9600;
                serialPort1.Open();
                PortBaglandi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Porta Bağlanılamadı", "Port Meşgul");
            }
        }

        void PortBaglandi()
        {
            button5.Enabled = false;
            groupBox1.Visible = false;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        string data;
        string rfidNo;

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            data = serialPort1.ReadLine();
            this.Invoke(new EventHandler(VeriGelince));
        }

        double mevcutPara;
        double odenenPara;
        string tarife;

        void VeriGelince(object sender, EventArgs e)
        {
            rfidNo = data.ToString().Remove(0, 6);
            if (bulunanPanel == 0)
            {
                textBox3.Text = rfidNo.ToString();
            }
            else if (bulunanPanel == 2)
            {
                label11.Text = "KAYITSIZ!!!";
                label12.Text = "";
                label20.Text = "KAYITSIZ!!!";

                label12.Text = rfidNo.ToString();
                baglan.Open();
                komut = new SqlCommand("Select *from KartKayit where KartID = '" + rfidNo.ToString() + "'", baglan);
                SqlDataReader oku = komut.ExecuteReader();

                while (oku.Read())
                {
                    label11.Text = oku["Isim"].ToString().Replace(" ", "") + " " + oku["Soyisim"].ToString().Replace(" ", "");
                    mevcutPara = double.Parse(oku["Para"].ToString());
                    label20.Text = mevcutPara + " ₺";
                }


                baglan.Close();
            }
            else if (bulunanPanel == 3)
            {
                mevcutPara = 0;
                odenenPara = 0;
                baglan.Open();
                komut = new SqlCommand("Select *from KartKayit where KartID = '" + rfidNo.ToString() + "'", baglan);
                komut.ExecuteNonQuery();
                SqlDataReader oku = komut.ExecuteReader();
                while (oku.Read())
                {
                    mevcutPara = double.Parse(oku["Para"].ToString());
                    tarife = oku["Tarife"].ToString().Replace(" ", "");
                }
                baglan.Close();
                if (tarife == "Öğrenci")
                {
                    label15.Text = tarife;
                    mevcutPara -= 1.25;
                    odenenPara = 1.25;
                }
                else if (tarife == "Tam")
                {
                    label15.Text = tarife;
                    mevcutPara = mevcutPara - 2.6;
                    odenenPara = 2.6;
                }
                else if (tarife == "Sosyal")
                {
                    label15.Text = tarife;
                    mevcutPara -= 1.85;
                    odenenPara = 1.85;
                }
                else if (tarife == "Ücretsiz")
                {
                    label15.Text = tarife;
                    mevcutPara -= 0;
                    odenenPara = 0;
                }

                if (mevcutPara < -5.5f)
                {
                    mevcutPara += odenenPara;
                    label16.Text = "TARİFE................................" + odenenPara;
                    label17.Text = "KALAN PARA................................" + mevcutPara;
                    label18.Text = "YETERSİZ BAKİYE! AVANS BULUNAMADI.";
                }
                else if (mevcutPara < 0)
                {
                    baglan.Open();
                    komut = new SqlCommand("update KartKayit set Para = '" + mevcutPara.ToString() + "' where KartID= '" + rfidNo.ToString() + "'", baglan);
                    komut.ExecuteNonQuery();
                    baglan.Close();
                    label16.Text = "TARİFE................................" + odenenPara;
                    label17.Text = "KALAN PARA................................" + mevcutPara;
                    label18.Text = "YETERSİZ BAKİYE! AVANS KULLANILDI.";
                }
                else
                {
                    baglan.Open();
                    komut = new SqlCommand("update KartKayit set Para = '" + mevcutPara.ToString() + "' where KartID= '" + rfidNo.ToString() + "'", baglan);
                    komut.ExecuteNonQuery();
                    baglan.Close();
                    label16.Text = "TARİFE................................" + odenenPara;
                    label17.Text = "KALAN PARA................................" + mevcutPara;
                    label18.Text = "KARTINIZI GÖSTERİNİZ!";
                }
            }
        }

        void PanelAc(int panel) //0 - kartEkle | 1 - portSec | 
        {
            bulunanPanel = panel;
            panels[panel].Visible = true;
            for (int i = 0; i < panels.Length; i++)
            {
                if (i != panel)
                {
                    panels[i].Visible = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label2.Text = "YENİ KART EKLEME PANELİ";
            PanelAc(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label2.Text = "PARA YÜKLEME PANELİ";
            PanelAc(2);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (label11.Text != "Kartınızı Gösteriniz!!!" || label12.Text != "Kartınızı Gösteriniz!!!")
            {
                if (label11.Text != "KAYITSIZ!!!" && label20.Text != "KAYITSIZ!!!")
                {
                    if (textBox4.Text != "")
                    {
                        try
                        {
                            float.Parse(textBox4.Text);
                            if (float.Parse(textBox4.Text) >= 5.0f && float.Parse(textBox4.Text) <= 500)
                            {
                                try
                                {
                                    baglan.Open();
                                    komut = new SqlCommand("update KartKayit set Para = '" + (mevcutPara + float.Parse(textBox4.Text)).ToString() + "' where KartID = '" + rfidNo.ToString() + "'", baglan);
                                    komut.ExecuteNonQuery();
                                    baglan.Close();
                                    MessageBox.Show("Para yüklemesi başarılı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    label11.Text = "Kartınızı Gösteriniz!!!";
                                    label12.Text = "Kartınızı Gösteriniz!!!";
                                    label20.Text = "Kartınızı Gösteriniz!!!";
                                    textBox4.Text = "";
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Lütfen alanları doldurun!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Lütfen sadece 5₺ ile 500₺ arası değerler girin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lütfen sadece sayı girin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lütfen yüklemek istediğiniz miktarı girin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Kayıtsız kişilere para yükleyemezsiniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Lütfen kartınızı gösteriniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label2.Text = "AKBİL OKUMA PANELİ";
            PanelAc(3);
            label11.Text = "Kartınızı Gösteriniz!!!";
            label12.Text = "Kartınızı Gösteriniz!!!";
            label20.Text = "Kartınızı Gösteriniz!!!";
        }

        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }
        }
    }
}