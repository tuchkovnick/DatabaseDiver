using DbDiver.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace DbDiver.Modules.Models
{
    public static class ItemsLoader
    {
        public static IEnumerable<DbSearchParameter> LoadItems()
        {            
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var filePath = ofd.FileName;
            var itemsStr = File.ReadAllLines(filePath);
            var result = new ObservableCollection<DbSearchParameter>();
            var regTable = new Regex($".*:");
            var regColumn = new Regex($":.*=>");
            var regSearch = new Regex($"=>.*//");
            var regDescr = new Regex($"//.*");
            foreach (var item in itemsStr)
            {
                var table = regTable.Match(item).ToString().Replace(":", string.Empty);
                var column = regColumn.Match(item).ToString().Replace(":", string.Empty).Replace("=>", string.Empty);
                var search = regSearch.Match(item).ToString().Replace("=>", string.Empty).Replace("//", string.Empty);
                var description = regDescr.Match(item).ToString().Replace("//", string.Empty);
                result.Add(new DbSearchParameter(table, column, search, description));
                
            }
            return result;
        }

        public static void SaveItems(IEnumerable<DbSearchParameter> searchParameters)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.ShowDialog();
                var filePath = sfd.FileName;
                var itemsStr = new List<string>();
                foreach (var item in searchParameters)
                {
                    itemsStr.Add($"{item.TableName}:{item.ColumnName}=>{item.SearchItem}//{item.Description}");
                }
                File.AppendAllLines(filePath, itemsStr);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
