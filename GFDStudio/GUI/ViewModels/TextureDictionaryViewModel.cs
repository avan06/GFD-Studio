﻿using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GFDLibrary;

namespace GFDStudio.GUI.ViewModels
{
    public class TextureDictionaryViewModel : ResourceViewModel<TextureDictionary>
    {
        public override TreeNodeViewModelMenuFlags ContextMenuFlags =>
            TreeNodeViewModelMenuFlags.Export | TreeNodeViewModelMenuFlags.Replace | TreeNodeViewModelMenuFlags.Move |
            TreeNodeViewModelMenuFlags.Rename | TreeNodeViewModelMenuFlags.Delete | TreeNodeViewModelMenuFlags.Add;

        public override TreeNodeViewModelFlags NodeFlags => TreeNodeViewModelFlags.Branch;

        protected internal TextureDictionaryViewModel( string text, TextureDictionary resource ) : base( text, resource )
        {
        }

        protected override void InitializeCore()
        {
            RegisterExportHandler<TextureDictionary>( path => Model.Save(  path ) );
            RegisterReplaceHandler<TextureDictionary>( Resource.Load<TextureDictionary> );

            RegisterAddHandler< Bitmap >( path => Model.Add( TextureEncoder.Encode( Path.GetFileNameWithoutExtension( path ) + ".dds", 
                TextureFormat.DDS, new Bitmap( path ) ) ) );

            RegisterAddHandler< Stream >( path => Model.Add( new Texture( Path.GetFileNameWithoutExtension( path ) + ".dds", 
                TextureFormat.DDS, File.ReadAllBytes( path ) ) ) );

            RegisterModelUpdateHandler( () =>
            {
                var textureDictionary = new TextureDictionary( Version );
                foreach ( TextureViewModel textureAdapter in Nodes )
                {
                    textureDictionary[textureAdapter.Name] = textureAdapter.Model;
                }

                return textureDictionary;
            } );

            RegisterCustomHandler( "Convert to field texture archive", () =>
            {
                using ( var dialog = new SaveFileDialog() )
                {
                    dialog.Filter = "Field Texture Archive (*.bin)|*.bin";
                    dialog.AutoUpgradeEnabled = true;
                    dialog.CheckPathExists = true;
                    dialog.FileName = Text;
                    dialog.OverwritePrompt = true;
                    dialog.Title = "Select a file to export to.";
                    dialog.ValidateNames = true;
                    dialog.AddExtension = true;

                    if ( dialog.ShowDialog() != DialogResult.OK )
                        return;

                    Replace( TextureDictionary.ConvertToFieldTextureArchive( Model, dialog.FileName ) );
                }
            } );
        }

        protected override void InitializeViewCore()
        {
            foreach ( var texture in Model.Textures )
            {
                var node = TreeNodeViewModelFactory.Create( texture.Name, texture );
                Nodes.Add( node );
            }
        }
    }
}
