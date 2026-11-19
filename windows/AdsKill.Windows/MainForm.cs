using System.Drawing;
using System.Windows.Forms;

namespace AdsKill.Windows;

public sealed class MainForm : Form
{
    private readonly Label _status;
    private readonly Label _rules;
    private readonly Label _message;
    private readonly Button _enable;
    private readonly Button _disable;
    private readonly Button _reward;
    private readonly Label _filterDiagnostic;
    private readonly Button _checkFiltering;
    private readonly Label _updateStatus;
    private readonly Button _checkUpdates;
    private readonly Button _downloadUpdate;
    private GithubReleaseUpdater.ReleaseInfo? _newRelease;
    private readonly CancellationTokenSource _updateCancel = new();
    private readonly System.Windows.Forms.Timer _timer;
    private DateTimeOffset? _rewardDeadline;

    public MainForm()
    {
        Text = "Ads Kill — Windows 0.2.1 beta";
        Width = 635;
        Height = 835;
        MinimumSize = new Size(580, 465);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(247, 250, 253);

        var outer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(24)
        };
        Controls.Add(outer);

        Label AddLabel(string text, int size, bool bold = false, int height = 44)
        {
            var label = new Label
            {
                AutoSize = false,
                Width = 545,
                Height = height,
                Text = text,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.FromArgb(25, 45, 67),
                Margin = new Padding(0, 5, 0, 7)
            };
            outer.Controls.Add(label);
            return label;
        }

        AddLabel("Ads Kill", 25, true, 49);
        AddLabel("Filtro HOSTS (30 domini) • Windows 11 x64 • beta 0.2.1", 11, false, 34);
        _status = AddLabel("Verifica protezione...", 17, true, 39);
        _rules = AddLabel("Regole: " + HostsSections.Domains.Length + " domini", 11, false, 32);

        var buttons = new FlowLayoutPanel
        {
            Width = 545, Height = 52, FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false, Margin = new Padding(0, 8, 0, 8)
        };
        outer.Controls.Add(buttons);

        Button AddButton(string caption, int width)
        {
            var button = new Button
            {
                Text = caption,
                AutoSize = false,
                Width = width,
                Height = 42,
                FlatStyle = FlatStyle.System,
                Font = new Font("Segoe UI", 10)
            };
            buttons.Controls.Add(button);
            return button;
        }

        _enable = AddButton("Attiva blocco", 150);
        _disable = AddButton("Disattiva blocco", 158);
        _reward = AddButton("Ricompensa: 60 s", 175);
        _enable.Click += (_, _) => ChangeProtection(true);
        _disable.Click += (_, _) =>
        {
            _rewardDeadline = null;
            ChangeProtection(false);
        };
        _reward.Click += (_, _) => BeginReward();

        _message = AddLabel("", 11, false, 73);
        AddLabel("VERIFICA REALE DEL FILTRO WINDOWS", 15, true, 35);
        _checkFiltering = new Button
        {
            Text = "Verifica regole HOSTS e DNS",
            Width = 282, Height = 42
        };
        outer.Controls.Add(_checkFiltering);
        _filterDiagnostic = AddLabel(
            "Il pulsante Protezione attiva conferma soltanto le regole HOSTS. " +
            "Non conferma il blocco di tutte le pubblicita, soprattutto dentro emulatori Android.",
            10, false, 128);
        _checkFiltering.Click += async (_, _) => await CheckFilterAsync();
        AddLabel("WeWard in WEmulDroid/emulatore Android: il sistema Android puo ignorare " +
            "il file HOSTS del PC. Questa beta Windows NON filtra il traffico dentro " +
            "l'emulatore. Per testare WeWard li serve un blocco configurato nel sistema Android.",
            10, false, 110);
        AddLabel("AGGIORNAMENTI ADS KILL", 15, true, 35);
        _updateStatus = AddLabel("Controllo automatico delle beta su GitHub...", 11, false, 45);
        var updatesRow = new FlowLayoutPanel {
            Width = 545, Height = 54, FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        outer.Controls.Add(updatesRow);
        _checkUpdates = new Button { Text = "Controlla aggiornamenti",
            Width = 247, Height = 42 };
        _downloadUpdate = new Button { Text = "Scarica nuova beta",
            Width = 247, Height = 42, Enabled = false };
        updatesRow.Controls.Add(_checkUpdates);
        updatesRow.Controls.Add(_downloadUpdate);
        _checkUpdates.Click += async (_, _) => await CheckUpdatesAsync();
        _downloadUpdate.Click += async (_, _) => await DownloadUpdateAsync();
        Shown += async (_, _) => await CheckUpdatesAsync();
        AddLabel("Controllo delle versioni all'avvio; scaricamento su conferma "
            + "con verifica SHA-256. L'installazione resta manuale fino "
            + "all'introduzione di una firma digitale permanente.",
            10, false, 80);
        AddLabel("Nessuna VPN, driver, estensione browser o account richiesti.", 10, false, 37);
        AddLabel("Il filtro usa 0.0.0.0 e :: per domini pubblicitari espliciti. "
            + "Non elimina gli annunci incorporati nei video e può non funzionare con DNS sicuri o cache proprie.",
            10, false, 72);
        AddLabel("Le modifiche richiedono autorizzazione amministratore. "
            + "Ads Kill conserva il file HOSTS originale in C:\\ProgramData\\AdsKill\\backups. "
            + "Il pulsante Disattiva rimuove SOLO le proprie righe.",
            10, false, 78);

        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += (_, _) => TickTimer();
        _timer.Start();
        RefreshState();
    }


    private async Task CheckFilterAsync()
    {
        _checkFiltering.Enabled = false;
        _filterDiagnostic.Text = "Verifica in corso: risoluzione DNS Windows su 4 domini campione...";
        try
        {
            _filterDiagnostic.Text = await HostsDiagnostics.RunAsync(_updateCancel.Token);
        }
        catch (OperationCanceledException) { _filterDiagnostic.Text = "Verifica annullata."; }
        catch (Exception ex) { _filterDiagnostic.Text = "Verifica non riuscita: " + ex.Message; }
        finally { if (!IsDisposed && !Disposing) _checkFiltering.Enabled = true; }
    }

    private async Task CheckUpdatesAsync()
    {
        _checkUpdates.Enabled = false;
        _downloadUpdate.Enabled = false;
        _updateStatus.Text = "Controllo GitHub in corso...";
        try
        {
            _newRelease = await GithubReleaseUpdater.CheckAsync(_updateCancel.Token);
            if (_newRelease is null)
                _updateStatus.Text = "Nessuna nuova versione Windows verificabile.";
            else
            {
                _updateStatus.Text = "Disponibile: " + _newRelease.Tag;
                _downloadUpdate.Enabled = true;
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e) { _updateStatus.Text = "Controllo non riuscito: " + e.Message; }
        finally { if (!IsDisposed && !Disposing) _checkUpdates.Enabled = true; }
    }

    private async Task DownloadUpdateAsync()
    {
        if (_newRelease is null) return;
        var release = _newRelease;
        if (MessageBox.Show(this,
            "Scaricare " + release.Tag + " da GitHub? La firma persistente non "
                + "è ancora configurata: l'installazione resterà manuale.",
            "Ads Kill — beta Windows", MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) != DialogResult.Yes) return;
        _downloadUpdate.Enabled = false;
        _checkUpdates.Enabled = false;
        try
        {
            _updateStatus.Text = "Scaricamento e verifica SHA-256...";
            var file = await GithubReleaseUpdater.DownloadAsync(release, _updateCancel.Token);
            _updateStatus.Text = "ZIP verificato in: " + file;
            MessageBox.Show(this, "Download verificato. Chiudi Ads Kill, estrai "
                + "lo ZIP e apri il nuovo AdsKill.Windows.exe.\n\n" + file,
                "Aggiornamento pronto", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                FileName = "explorer.exe",
                Arguments = "/select,\"" + file + "\"",
                UseShellExecute = true
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception e) { _updateStatus.Text = "Download non completato: " + e.Message; }
        finally
        {
            if (!IsDisposed && !Disposing) {
                _checkUpdates.Enabled = true;
                _downloadUpdate.Enabled = true;
            }
        }
    }

    private void ChangeProtection(bool protect)
    {
        try
        {
            var result = HostsStore.SetProtected(protect);
            _message.Text = result.Changed
                ? (protect ? "Protezione attivata." : "Protezione disattivata.")
                    + " Backup: " + result.BackupPath
                : "Nessuna modifica: lo stato era già quello richiesto.";
            if (result.DnsWarning is not null) _message.Text += "\n" + result.DnsWarning;
        }
        catch (Exception ex)
        {
            _message.Text = "Operazione non completata.";
            MessageBox.Show(this, ex.Message,
                "Ads Kill — modifica non riuscita", MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        RefreshState();
    }

    private void BeginReward()
    {
        try
        {
            HostsStore.SetProtected(false);
            _rewardDeadline = DateTimeOffset.UtcNow.AddSeconds(60);
            _message.Text = "Blocco sospeso per 60 secondi. "
                + "Mantieni questa finestra aperta: ripristino automatico alla scadenza.";
            RefreshState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ads Kill — errore",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TickTimer()
    {
        if (_rewardDeadline is null)
        {
            RefreshState();
            return;
        }

        var remaining = _rewardDeadline.Value - DateTimeOffset.UtcNow;
        if (remaining > TimeSpan.Zero)
        {
            _message.Text = "Ricompensa: " + Math.Ceiling(remaining.TotalSeconds)
                + " secondi rimasti. Lascia Ads Kill aperto.";
            RefreshState();
            return;
        }

        try
        {
            HostsStore.SetProtected(true);
            _rewardDeadline = null;
            _message.Text = "Protezione riattivata automaticamente.";
        }
        catch (Exception ex)
        {
            _rewardDeadline = null;
            _message.Text = "Riattivazione non riuscita: premi Attiva blocco.";
            MessageBox.Show(this, ex.Message, "Riattivazione richiesta",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        RefreshState();
    }

    private void RefreshState()
    {
        try
        {
            bool enabled = HostsStore.IsProtected();
            _status.Text = enabled ? "● Regole HOSTS installate" : "○ Regole HOSTS disattivate";
            _status.ForeColor = enabled ? Color.DarkGreen : Color.DarkRed;
            _rules.Text = enabled
                ? HostsSections.Domains.Length + " domini pubblicitari nel file HOSTS"
                : "Regole Ads Kill non presenti nel file HOSTS";
            _enable.Enabled = !enabled && _rewardDeadline is null;
            _disable.Enabled = enabled || _rewardDeadline is not null;
            _reward.Enabled = enabled && _rewardDeadline is null;
        }
        catch (Exception ex)
        {
            _status.Text = "Impossibile leggere il file HOSTS";
            _rules.Text = ex.Message;
            _enable.Enabled = false;
            _disable.Enabled = false;
            _reward.Enabled = false;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _updateCancel.Cancel();
        _timer.Stop();
        if (_rewardDeadline is not null)
        {
            try
            {
                HostsStore.SetProtected(true);
                _rewardDeadline = null;
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                _timer.Start();
                MessageBox.Show(this,
                    "Non posso chiudere mentre il blocco è sospeso e il ripristino fallisce: "
                    + ex.Message + "\nPremi Attiva blocco oppure Disattiva blocco per scegliere.",
                    "Ads Kill — ripristino", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        base.OnFormClosing(e);
    }
}
