using NAudio.Wave;
using Newtonsoft.Json; // JSON ayrıştırma için
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Vosk;

namespace speech_to_text
{
    public partial class Form1 : Form
    {
        // Vosk nesneleri
        private VoskRecognizer voskRecognizer;
        private Model voskModel;

        // NAudio nesneleri (Mikrofon girişi için)
        private WaveInEvent waveIn;

        // Ayarlar
        private const int SAMPLE_RATE = 16000;
        private const string TURKISH_MODEL_PATH = "vosk-model-small-tr-0.3"; // Türkçe model klasörünüzün adı
        
        private const string ALMANCA_MODEL_PATH = "vosk-model-small-de-0.15";
        
        public Form1()
        {
            InitializeComponent();
            AttachLanguageComboHandler();
            VoskSisteminiHazirla();
        }

        private void AttachLanguageComboHandler()
        {
            try
            {
                var cmb = this.Controls.Find("cmbLanguage", true).FirstOrDefault() as ComboBox;
                if (cmb != null)
                {
                    cmb.SelectedIndexChanged -= CmbLanguage_SelectedIndexChanged;
                    cmb.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
                }
            }
            catch
            {
                // Sessizce başarısız ol — formda combo yoksa zorunlu değil
            }
        }

        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Mevcut modeli ve recognizer'ı serbest bırakıp seçime göre yeniden yükle
            try
            {
                if (waveIn != null)
                {
                    waveIn.StopRecording();
                    btnBaslat.Enabled = true;
                    btnDurdur.Enabled = false;
                }

                if (voskRecognizer != null)
                {
                    voskRecognizer.Dispose();
                    voskRecognizer = null;
                }

                // Vosk modelini elle dispose edenek isterseniz açın. (Bazı Vosk sürümlerinde gerekmeyebilir)
                // if (voskModel != null) { voskModel.Dispose(); voskModel = null; }
            }
            catch { }

            DurumGuncelle("Dil değiştirildi — model yeniden yükleniyor...");
            VoskSisteminiHazirla();
        }

        private string GetModelFolderForSelectedLanguage()
        {
            try
            {
                var cmb = this.Controls.Find("cmbLanguage", true).FirstOrDefault() as ComboBox;
                if (cmb != null)
                {
                    var selected = (cmb.SelectedItem ?? cmb.Text ?? "").ToString().Trim();
                    
                        
                    
                        
                    
                    // Eski İngilizce kontrolü yerine (veya yanına):

                     if (string.Equals(selected, "Deutsch", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(selected, "Almanca", StringComparison.OrdinalIgnoreCase))
                    {
                        return ALMANCA_MODEL_PATH; // Yeni tanımladığınız sabiti döndürün
                    }


                }
            }
            catch { }

            // Varsayılan Türkçe
            return TURKISH_MODEL_PATH;
        }

        private void VoskSisteminiHazirla()
        {
            try
            {
                // Vosk loglarını sessize al
                Vosk.Vosk.SetLogLevel(-1);

                // Model seçimi: eğer formda bir cmbLanguage varsa onun seçimine göre, yoksa varsayılan Türkçe
                string modelFolderName = GetModelFolderForSelectedLanguage();
                string modelPath = System.IO.Path.Combine(Application.StartupPath, modelFolderName);

                // Çalışma dizinini göster (debug için)
                DurumGuncelle($"Çalışma dizini: {Application.StartupPath}");

                if (!System.IO.Directory.Exists(modelPath))
                {
                    DurumGuncelle($"HATA: Model klasörü bulunamadı: {modelPath}");
                    MessageBox.Show($"Seçili model ({modelFolderName}) bulunamadı.\nBeklenen konum: {modelPath}\n\nÇözüm:\n1) Modeli indirin: https://alphacephei.com/vosk/models\n2) Klasörü proje çıkış dizinine (bin\\Debug veya bin\\Release) kopyalayın\n3) Veya proje köküne koyup __Project Properties > Build Events__ içine post-build kopyalama ekleyin.", "Model Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Önceki model varsa serbest bırak (gerekiyorsa)
                if (voskRecognizer != null)
                {
                    voskRecognizer.Dispose();
                    voskRecognizer = null;
                }

                // Modeli yükle
                voskModel = new Model(modelPath);
                voskRecognizer = new VoskRecognizer(voskModel, SAMPLE_RATE);

                // Mikrofonu hazırla (eğer daha önce oluşturulmadıysa)
                if (waveIn == null)
                {
                    waveIn = new WaveInEvent();
                    waveIn.WaveFormat = new WaveFormat(SAMPLE_RATE, 1);
                    waveIn.DataAvailable += WaveIn_DataAvailable;
                    waveIn.RecordingStopped += WaveIn_RecordingStopped;
                }

                DurumGuncelle($"Vosk hazır. Yüklenen model: {modelFolderName}");
                btnBaslat.Enabled = true;
                btnDurdur.Enabled = false;
            }
            catch (Exception ex)
            {
                DurumGuncelle($"Kritik Hata: {ex.Message}");
                MessageBox.Show($"Sistem hazırlanırken hata oluştu:\n{ex.Message}", "Hata",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Mikrofon verisini Vosk Recognizer'a gönder
            if (voskRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                // *Tamamlanmış bir cümle tanındı*
                string resultJson = voskRecognizer.Result();
                ProcessVoskResult(resultJson);
                DurumGuncelle("Tanındı: ✅ Dinliyorum...");
            }
            else
            {
                // *Parçalı (anlık) sonuçları almak için:*
                // string partialResultJson = voskRecognizer.PartialResult();
                // ProcessVoskPartialResult(partialResultJson); 
            }
        }

        private void ProcessVoskResult(string resultJson)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(ProcessVoskResult), resultJson);
                return;
            }

            try
            {
                // JSON.NET ile sonucu ayrıştır
                dynamic result = JsonConvert.DeserializeObject(resultJson);
                string taninanMetin = result?.text != null ? result.text.ToString() : "[Boş Sonuç]";

                if (!string.IsNullOrWhiteSpace(taninanMetin))
                {
                    string zamanDamgasi = DateTime.Now.ToString("HH:mm:ss");
                    string yeniSatir = $"[{zamanDamgasi}] {taninanMetin}";

                    txtSonuc.AppendText(yeniSatir + Environment.NewLine);
                    txtSonuc.SelectionStart = txtSonuc.Text.Length;
                    txtSonuc.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                // JSON ayrıştırma hatası, genelde çok nadir olur.
                DurumGuncelle($"JSON ayrıştırma hatası: {ex.Message}");
            }
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                DurumGuncelle($"Kayıt durduruldu (Hata: {e.Exception.Message})");
            }
            else
            {
                DurumGuncelle("Durduruldu ⏸");
            }

            // Son kalan veriyi işle
            if (voskRecognizer != null)
            {
                string finalResultJson = voskRecognizer.FinalResult();
                ProcessVoskResult(finalResultJson);
            }
        }

        // --- Buton Olayları ---

        private void btnBaslat_Click(object sender, EventArgs e)
        {
            if (waveIn != null)
            {
                try
                {
                    if (WaveIn.DeviceCount < 1)
                    {
                        MessageBox.Show("Mikrofon bulunamadı. Lütfen bir mikrofon bağlayın.", "Donanım Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    waveIn.StartRecording();
                    DurumGuncelle("Dinliyorum... 🎤 Konuşun!");
                    btnBaslat.Enabled = false;
                    btnDurdur.Enabled = true;
                }
                catch (Exception ex)
                {
                    DurumGuncelle($"Başlatma Hatası: {ex.Message}");
                    MessageBox.Show($"Mikrofon başlatılamadı:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDurdur_Click(object sender, EventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                btnBaslat.Enabled = true;
                btnDurdur.Enabled = false;
            }
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            txtSonuc.Clear();
        }

        private void DurumGuncelle(string mesaj)
        {
            if (lblDurum.InvokeRequired)
            {
                lblDurum.Invoke(new Action<string>(DurumGuncelle), mesaj);
                return;
            }

            lblDurum.Text = mesaj;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Uygulama kapanırken kaynakları serbest bırak
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
            }

            if (voskRecognizer != null)
            {
                voskRecognizer.Dispose();
            }

            // Vosk Modelini de serbest bırakın (Vosk'un kendisi yönetebilir ancak iyi bir pratiktir)
            // if (voskModel != null)
            // {
            //     voskModel.Dispose();
            // }
        }
    }
}
