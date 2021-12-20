using App_UI.Commands;
using App_UI.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace App_UI.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Membres
        // Mettre les membres ici

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private UsersViewModel usersViewModel;

        private string filename;

        private IFileDialogService openFileDialog;
        private IFileDialogService saveFileDialog;
        private MessageBoxDialogService confirmDialog;

        #endregion

        #region Propriétés
        // Mettre les propriétés ici
        /// <summary>
        /// Model actuellement affiché
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// String contenant le nom du fichier
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        public List<BaseViewModel> ViewModels
        {
            get
            {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels;
            }
        }

        #endregion

        #region Commandes

        public DelegateCommand<string> SaveFileCommand { get; set; }
        public DelegateCommand<string> OpenFileCommand { get; set; }
        public DelegateCommand<string> NewRecordCommand { get; set; }

        /// <summary>
        /// Commande pour changer la page à afficher
        /// </summary>
        public DelegateCommand<string> ChangePageCommand { get; set; }

        /// <summary>
        /// TODO 01a : Compléter l'ImportCommand
        /// </summary>
        public DelegateCommand<string> ImportCommand { get; set; }

        /// <summary>
        /// Commande exécutée pour exporter les données vers un fichier
        /// </summary>
        public DelegateCommand<string> ExportCommand { get; set; }

        /// <summary>
        /// TODO 04a : Compléter ChangeLanguageCommand
        /// </summary>
        public DelegateCommand<string> ChangeLanguageCommand { get; set; }

        public DelegateCommand<object> changeLanguageEnCommand { get; set; }
        public DelegateCommand<object> changeLanguageFrCommand { get; set; }




        #endregion


        public ApplicationViewModel(FileDialogService _openFileDialog, 
                                    FileDialogService _saveFileDialog, 
                                    MessageBoxDialogService _confirmDeleteDialog)
        {
            openFileDialog = _openFileDialog;
            saveFileDialog = _saveFileDialog;
            confirmDialog = _confirmDeleteDialog;

            initCommands();            
            initViewModels();
        }

        #region Méthodes

        /// <summary>
        /// Initialisation des commandes
        /// </summary>
        private void initCommands()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);
            ExportCommand = new DelegateCommand<string>(ExportData);
            NewRecordCommand = new DelegateCommand<string>(RecordCreate);
            ImportCommand =  new DelegateCommand<string>(ImportData);
            changeLanguageEnCommand = new DelegateCommand<object>(changeLanguageEn, CanChangeLanguage);
            changeLanguageFrCommand = new DelegateCommand<object>(changeLanguageFr, CanChangeLanguage);

        }
        public void Restart(object arg)
        {

            var filename = Application.ResourceAssembly.Location;
            var newFile = Path.ChangeExtension(filename, ".exe");
            Process.Start(newFile);
            Application.Current.Shutdown();
        }


        private bool CanChangeLanguage(object arg) => true;
        private void changeLanguageFr(object arg)
        {
            string param = Properties.Settings.Default.Language;
            if (param == "fr")
            {
                MessageBoxResult messageErrorBoxResult = System.Windows.MessageBox.Show("Deja en francais!");
            }
            else
            {


                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Change language to french?", "Language", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.Language = "fr";

                    Properties.Settings.Default.Save();
                    Restart(arg);
                }
            }
        }
        private void changeLanguageEn(object arg)
        {
            string param = Properties.Settings.Default.Language;
            if (param == "en")
            {
                MessageBoxResult messageErrorBoxResult = System.Windows.MessageBox.Show("Already in english");
            }
            else
            {


                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Changer le language pour l'anglais?", "Language", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.Language = "en";

                    Properties.Settings.Default.Save();
                    Restart(arg);
                }
            }
        }
        
        private void RecordCreate(string obj)
        {
            usersViewModel?.CreateEmptyUser();
        }

        private void ExportData(string obj)
        {
            /// TODO 02a : Compléter ExportData
            /// Utiliser PeopleDataService.Instance.GetAllAsJson() pour récupérer le json
            /// 
            using (StreamWriter outputFile = new StreamWriter(obj))
            {

                outputFile.WriteLine(PeopleDataService.Instance.GetAllAsJson(PeopleDataService.Instance.GetAllAsJson("")));
            }
            
        }

        private async void ImportData(string obj)
        {
            /// TODO 01b : Compléter la commande d'importation
            /// Utiliser PeopleDataService.Instance.SetAllFromJson(string allContent)
            /// 



            String filevalue = "";

            try
            {

                using (var sr = new StreamReader(obj))
                {
                    filevalue = sr.ReadToEnd();

                    PeopleDataService.Instance.SetAllFromJson(filevalue);
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("fivchier non lu");
                Console.WriteLine(e.Message);
            }
          
        }

        private void initViewModels()
        {
            usersViewModel = new UsersViewModel(PeopleDataService.Instance);

            usersViewModel.SetDeleteDialog(confirmDialog);

            CurrentViewModel = usersViewModel;

            var configurationViewModel = new ConfigurationViewModel();

            ViewModels.Add(configurationViewModel);
            ViewModels.Add(usersViewModel);
        }

        private void ChangePage(string name)
        {
            var vm = ViewModels.Find(vm => vm.Name == name);
            CurrentViewModel = vm;
        }

        private void ChangeLanguage(string language)
        {
            Properties.Settings.Default.Language = language;
            Properties.Settings.Default.Save();

            var msg = (MessageBoxDialogService)confirmDialog.Clone();

            msg.Message = $"{Properties.Resources.msg_restart}";
            msg.Caption = $"{Properties.Resources.msg_warning}";
            msg.Buttons = System.Windows.MessageBoxButton.OK;
            _ = msg.ShowDialog();

            var filename = System.Windows.Application.ResourceAssembly.Location;
            var newFile = Path.ChangeExtension(filename, ".exe");
            Process.Start(newFile);
            System.Windows.Application.Current.Shutdown();

        }

        #endregion
    }
}