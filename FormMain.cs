using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace CssSprite
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// �汾��
        /// </summary>
        public const string CurentVersion = "4.3.0.0";

        /// <summary>
        /// ��������ַ
        /// </summary>
        private const string NetUrl = "https://csssprite.herokuapp.com/";
		
        /// <summary>
        /// ����
        /// </summary>
		private int orderby_mode = 0; //����ʽ
		private int orderby_direction = 0; //������
        
        private VersionInfo newVersion;
        private List<ImageInfo> _imgList;
        private string dialogFile = string.Empty;
        private string basePath;
        internal class ImageInfo
        {
            internal ImageInfo(Image img, string name, string fileName)
            {
                Image = img;
                Name = name;
                FileName = fileName;
            }

            internal readonly Image Image;
            internal readonly string Name;
            internal readonly string FileName;
        }

        private Thread thread;
        public FormMain()
        {
            InitializeComponent();
            panelImages.MouseWheel += panelImages_MouseWheel;
            panelImages.MouseHover += panelImages_MouseHover;
            panelImages.MouseDown += panelImages_MouseDown;
            panelImages.MouseMove += panelImages_MouseMove;
            panelImages.MouseUp += panelImages_MouseUp;

            panelImages.KeyDown += panelImages_KeyDown;
            panelImages.LostFocus += panelImages_LostFocus;
            ThreadStart th = new ThreadStart(GetService);
            thread = new Thread(th);
            thread.Start();
            comboBoxImgType.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxOrderby.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        void panelImages_LostFocus(object sender, EventArgs e)
        {
            list = null; 
        }

        Point keyDownPoint;
        void panelImages_KeyDown(object sender, KeyEventArgs e)
        {
            if ((_selectedPicture != null && list == null) || (_selectedPicture != null && list.Count == 0))
            {
                keyDownPoint = new Point(_selectedPicture.Location.X, _selectedPicture.Location.Y);
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        if (keyDownPoint.X > 0)
                        {
                            keyDownPoint.X -= 1;
                        }
                        break;
                    case Keys.Up:
                        if (keyDownPoint.Y > 0)
                        {
                            keyDownPoint.Y -= 1;
                        }
                        break;
                    case Keys.Down:
                        keyDownPoint.Y += 1;
                        break;
                    case Keys.Right:
                        keyDownPoint.X += 1;
                        break;
                }
                _selectedPicture.Location = keyDownPoint;
            }
            else if (list != null)
            {
                foreach (PictureBox pb in list)
                {
                    keyDownPoint = new Point(pb.Location.X, pb.Location.Y);
                    switch (e.KeyCode)
                    {
                        case Keys.Left:
                            if (keyDownPoint.X > 0)
                            {
                                keyDownPoint.X -= 1;
                            }
                            else
                            {
                                return;
                            }
                            break;
                        case Keys.Up:
                            if (keyDownPoint.Y > 0)
                            {
                                keyDownPoint.Y -= 1;
                            }
                            else
                            {
                                return;
                            }
                            break;
                        case Keys.Down:
                            keyDownPoint.Y += 1;
                            break;
                        case Keys.Right:
                            keyDownPoint.X += 1;
                            break;
                    }
                    pb.Location = keyDownPoint;
                }
                DrawRectangle(list);
            }
            SetCssText();
        }

        private delegate void EnableButtonCallBack();

        private void ShowBtnUpdate(){
            this.Text = "Css����ͼ�ϲ�����(���¸��£������·����°�ť����)";
            btnUpdate.Visible = true;
        }

        void GetService()
        {
            try
            {
                var version = new VersionInfo() { Version=CurentVersion};
                var newVersionStr = httpClass.HttpPost(NetUrl, "data=" + XmlSerializer.XMLSerialize<VersionInfo>(version));
                newVersion = XmlSerializer.DeXMLSerialize<VersionInfo>(newVersionStr);
                if (newVersion.Version != version.Version) 
                {
                    this.Invoke(new EnableButtonCallBack(ShowBtnUpdate));
                }
                thread.Abort();
            }
            catch
            {
                
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var updateForm = new Update(CurentVersion, newVersion);
            updateForm.ShowDialog();
        }

        /// <summary>
        /// ���ĳ�ʼλ��
        /// </summary>
        Point _panelPoint;
        /// <summary>
        /// �Ƿ����϶�
        /// </summary>
        bool _isSelect = false;
        /// <summary>
        /// ����
        /// </summary>
        Graphics g;
        /// <summary>
        /// ��ɫ��
        /// </summary>
        Pen pen;
        /// <summary>
        /// ����
        /// </summary>
        Area area;
        /// <summary>
        /// ��ʱװ��ѡ��ͼƬ�б�
        /// </summary>
        List<PictureBox> list;
        void panelImages_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                list = null;
                pen = new Pen(Color.Blue);
                g = panelImages.CreateGraphics();
                _panelPoint = new Point(e.X, e.Y);
                _isSelect = true;
                area = new Area();
            }
            else
            {
                _isSelect = false;
            }
        }
        
        void panelImages_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelect && list == null)
            {
                panelImages.Refresh();
                if (e.X > _panelPoint.X && e.Y > _panelPoint.Y)
                {
                    area.ZeroPoint = new Point(_panelPoint.X, _panelPoint.Y);
                    area.Height = e.Y - _panelPoint.Y;
                    area.Width = e.X - _panelPoint.X;
                }
                else if (e.X < _panelPoint.X && e.Y < _panelPoint.Y)
                {
                    area.ZeroPoint = new Point(e.X, e.Y);
                    area.Height = _panelPoint.Y - e.Y;
                    area.Width = _panelPoint.X - e.X;
                }
                else if (e.X < _panelPoint.X && e.Y > _panelPoint.Y)
                {
                    area.ZeroPoint = new Point(e.X, _panelPoint.Y);
                    area.Width = _panelPoint.X - e.X;
                    area.Height = e.Y - _panelPoint.Y;
                }
                else
                {
                    area.ZeroPoint = new Point(_panelPoint.X, e.Y);
                    area.Width = e.X - _panelPoint.X;
                    area.Height = _panelPoint.Y - e.Y;
                }
                g.DrawRectangle(pen, area.ZeroPoint.X, area.ZeroPoint.Y, area.Width, area.Height);
            }
            else if (_isSelect && list != null)
            {
                
            }
        }

        void panelImages_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isSelect) 
            {
                _isSelect = false;
                list = new List<PictureBox>();

                foreach(PictureBox pb in panelImages.Controls)
                {
                    if (pb.Location.X > area.ZeroPoint.X && pb.Location.Y > area.ZeroPoint.Y &&
                        pb.Location.X + pb.Width < area.ZeroPoint.X + area.Width &&
                        pb.Location.Y + pb.Height < area.ZeroPoint.Y + area.Height
                        ) 
                    {
                        list.Add(pb);
                    }
                }
                DrawRectangle(list);
            }
        }

        /// <summary>
        /// �ػ���α߿�
        /// </summary>
        /// <param name="lists"></param>
        void DrawRectangle(List<PictureBox> list) 
        {
            var size = GetEdgeSize(list);
            panelImages.Refresh();
            g.DrawRectangle(pen, size.MinWidth, size.MinHeight, size.MaxWidth - size.MinWidth, size.MaxHeight - size.MinHeight);
        }

        /// <summary>
        ///��ȡ�����С�ߴ�
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private EdgeSize GetEdgeSize(List<PictureBox> list)
        {
            var size = new EdgeSize();
            foreach (PictureBox pb in list)
            {
                if (list.IndexOf(pb) == 0)
                {
                    size.MinWidth = pb.Location.X;
                    size.MinHeight = pb.Location.Y;
                }
                size.MinWidth = Math.Min(size.MinWidth, pb.Location.X);
                size.MinHeight = Math.Min(size.MinHeight, pb.Location.Y);
                size.MaxWidth = Math.Max(size.MaxWidth, pb.Location.X + pb.Image.Width);
                size.MaxHeight = Math.Max(size.MaxHeight, pb.Location.Y + pb.Image.Height);
            }
            return size;
        }

        void panelImages_MouseHover(object sender, EventArgs e)
        {
            panelImages.Focus();
        }

        void panelImages_MouseWheel(object sender, MouseEventArgs e)
        {
            panelImages.ResumeLayout(false);
            panelImages.Refresh();
        }


        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (!OpenFile(false)) {
                return;
            }
            DialogResult dr = openFileDialog.ShowDialog();
            if (DialogResult.OK == dr && openFileDialog.FileNames.Length > 0)
            {
                if (!AssertFiles())
                {
                    return;
                }
                basePath = Path.GetDirectoryName(openFileDialog.FileName);
                folderBrowserDialog.SelectedPath = basePath;
                LoadImages(openFileDialog.FileNames);
                ButtonVRange_Click(null, EventArgs.Empty);
                SetBase64();
            }
        }

        private void btnSprite_Click(object sender, EventArgs e)
        {
            if (!OpenFile(true))
            {
                return;
            }
            DialogResult dr = openFileDialog.ShowDialog();
            if (DialogResult.OK == dr && openFileDialog.FileNames.Length > 0)
            {
                basePath = Path.GetDirectoryName(openFileDialog.FileName);
                folderBrowserDialog.SelectedPath = basePath;
                var spriteFile=new SpriteFile();
                try
                {
                    spriteFile = (SpriteFile)XmlSerializer.LoadFromXml(openFileDialog.FileNames[0], spriteFile.GetType());
                    if (_imgList == null)
                    {
                        _imgList = new List<ImageInfo>();
                    }
                    var noFile = "��Щ�ļ������ڣ�" + Environment.NewLine;
                    var hasFile=false;
                    foreach (Sprite s in spriteFile.SpriteList)
                    {
                        var path=folderBrowserDialog.SelectedPath+"\\"+ s.Path;
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            string imgName = Path.GetFileNameWithoutExtension(s.Path);
                            ImageInfo imgInfo = new ImageInfo(img, imgName, path);
                            img.Tag = imgInfo;
                            _imgList.Add(imgInfo);
                            AddPictureBox(img, s.LocationX, s.LocationY);
                        }
                        else 
                        {
                            hasFile=true;
                            noFile += path + Environment.NewLine;
                        }
                    }
                    if (hasFile) 
                    {
                        MessageBox.Show(noFile, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    txtDir.Text = spriteFile.CssFileName;
                    txtName.Text = spriteFile.ImageName;
                    chkBoxPhone.Checked = spriteFile.IsPhone;
                    comboBoxImgType.Text = spriteFile.SpriteImgFileType == null ? "png" : spriteFile.SpriteImgFileType;
                    panelImages.ResumeLayout(false);
                    SetCssText();
                    SetBase64();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ".sprite�ļ����𻵣��޷��򿪣�");
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Png�ļ�|*.png|Jpeg�ļ�|*.jpeg|Jpg�ļ�|*.jpg";
            openFileDialog.Multiselect = false;
            DialogResult dr = openFileDialog.ShowDialog();
            if (DialogResult.OK == dr && openFileDialog.FileNames.Length > 0)
            {
                if (_imgList == null)
                {
                    _imgList = new List<ImageInfo>();
                }
                var fileName = openFileDialog.FileName;
                
                if (!IsImgExists(fileName))
                {
                    Image img = Image.FromFile(fileName);
                    string imgName = Path.GetFileNameWithoutExtension(fileName);
                    ImageInfo imgInfo = new ImageInfo(img, imgName, fileName);
                    img.Tag = imgInfo;
                    _imgList.Add(imgInfo);
                    AddPictureBox(img, 0, 0);
                    SetBase64();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedPicture != null)
            { 
                var dr =  MessageBox.Show("ȷ��ɾ��ͼƬ��" + ((ImageInfo)_selectedPicture.Image.Tag).Name + " ��", "ѯ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes) {
                    foreach (ImageInfo info in _imgList)
                    {
                        if (info.Image == _selectedPicture.Image)
                        {
                            _imgList.Remove(info);
                            break;
                        }
                    }
                    panelImages.Controls.Remove(_selectedPicture);
                    _selectedPicture = null;
                    SetCssText();
                    SetBase64();
                }
            }
            else
            {
                MessageBox.Show("��ѡ������Ҫ�Ƴ���ͼƬ��");
            }
        }

        /// <summary>
        /// �����Լ��Ի����ʼ��
        /// </summary>
        /// <param name="spriteFile"></param>
        private bool OpenFile(bool spriteFile) 
        {
            if (_imgList != null && _imgList.Count > 0)
            {
                DialogResult queryDr = MessageBox.Show("ȷʵҪ����ѡ��ͼƬ������ѡ��ͼƬ����ǰ��ͼƬ���ֽ���ʧ��", "ѯ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (queryDr == DialogResult.Yes)
                {
                    _imgList.Clear();
                    panelImages.Controls.Clear();
                }
                else
                {
                    return false ;
                }
            }
            if (spriteFile)
            {
                openFileDialog.Filter = "css sprite�ļ�|*.sprite";
                openFileDialog.Multiselect = false;
            }
            else {
                openFileDialog.Filter = "Png�ļ�|*.png|Jpeg�ļ�|*.jpeg|Jpg�ļ�|*.jpg";
                openFileDialog.Multiselect = true;
            }
            return true;
        }

        /// <summary>
        /// ����ͼƬ������
        /// </summary>
        /// <param name="imageFileNames"></param>
        private void LoadImages(string[] imageFileNames)
        {
            if (_imgList == null)
            {
                _imgList = new List<ImageInfo>();
            }
            foreach (string fileName in imageFileNames)
            {
                if (IsImgExists(fileName))
                {
                    continue;
                }
                Image img = Image.FromFile(fileName);
                string imgName = Path.GetFileNameWithoutExtension(fileName);
                ImageInfo imgInfo = new ImageInfo(img, imgName, fileName);
                img.Tag = imgInfo;
                _imgList.Add(imgInfo);
            }
            //_imgList.Sort(ImageComparison);
            _imgList.Sort((x, y) => { return -y.Name.CompareTo(x.Name); });
        }

        int ImageComparison(ImageInfo i1, ImageInfo i2)
        {
            return i1.Image.Width > i2.Image.Width ? 1 : (i1.Image.Width == i2.Image.Width ? 0 : -1);
        }

        /// <summary>
        /// ��֤�Ƿ��Ƕ���ļ�
        /// </summary>
        /// <returns></returns>
        private bool AssertFiles()
        {
            string[] files = openFileDialog.FileNames;
            if (files == null || (openFileDialog.Multiselect ? files.Length < 2 : files.Length <0))
            {
                MessageBox.Show("��ѡ����ͼƬ�ļ���");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ѡ�еĵ���ͼƬ
        /// </summary>
        private PictureBox _selectedPicture=null;

        string GetImgExt()
        {
            string ext = comboBoxImgType.Text.ToLower();
            if (ext == "png" || ext == "gif" || ext == "jpg" || ext == "jpeg")
            {
                return ext;
            }
            return "png";
        }

        
        //Сͼ���ŵ��
        private void ButtonVRange_Click(object sender, EventArgs e)
        {
            if (!AssertFiles()) return;
            panelImages.Controls.Clear();
            int left = 0;
            int top = 0;
            int currentHeight = 0;
            foreach (ImageInfo ii in _imgList)
            {
                Image img = ii.Image;
                left = 0;
                top = currentHeight;

                AddPictureBox(img, left, top);
                currentHeight += img.Height;
            }
            panelImages.ResumeLayout(false);
            SetCssText();
        }

        /// <summary>
        /// ����css���ı�
        /// </summary>
        public void SetCssText() {
            var _list=new List<PictureBox>();
             foreach (PictureBox pb in panelImages.Controls)
            {
                _list.Add(pb);
            }
            var edgeSize = GetEdgeSize(_list);
            var isPhone = chkBoxPhone.Checked;
            var tmpStr = "{0}"+txtName.Text + "[background:url(" + txtDir.Text + "/" + txtName.Text + "." + GetImgExt() + ") no-repeat {1};]" + Environment.NewLine;

            var sassStr = string.Empty;
            var cssStr = string.Empty;

            if (chkBoxPhone.Checked)
            {
                chkBoxPhone_CheckedChanged(null, EventArgs.Empty);
                sassStr = String.Format(tmpStr, "@mixin ", ";background-size:$_" + (edgeSize.MaxWidth - edgeSize.MinWidth) + " $_" + (edgeSize.MaxHeight - edgeSize.MinHeight)).Replace("[", "{").Replace("]", "}");
                cssStr = String.Format(tmpStr, ".", ";background-size:@_" + (edgeSize.MaxWidth - edgeSize.MinWidth) + " $_" + (edgeSize.MaxHeight - edgeSize.MinHeight)).Replace("[", "{").Replace("]", "}");
            }
            else {
                sassStr = String.Format(tmpStr, "@mixin ","").Replace("[", "{").Replace("]", "}");
                cssStr = String.Format(tmpStr, ".","").Replace("[", "{").Replace("]", "}");
            }
            
            foreach (PictureBox pb in panelImages.Controls)
            {
                sassStr += GetSassCss(pb.Image, pb.Left - edgeSize.MinWidth, pb.Top - edgeSize.MinHeight, true);
                cssStr += GetSassCss(pb.Image, pb.Left - edgeSize.MinWidth, pb.Top - edgeSize.MinHeight, false);
            }
            txtSass.Text = sassStr;
            txtCss.Text = cssStr;
        }

        /// <summary>
        /// �õ�sass����
        /// </summary>
        /// <param name="img">ͼƬ</param>
        /// <param name="left">��߾���</param>
        /// <param name="top">�ұ߾���</param>
        /// <returns></returns>
        string GetSassCss(Image img, int left, int top,bool isSass)
        {
            ImageInfo imgInfo = (ImageInfo)img.Tag;
            var isPhone = chkBoxPhone.Checked;
            var unit = "px";
            var sassPrefix = string.Empty;
            var lessPrefix = string.Empty;
            if (isPhone)
            {
                unit = "";
                lessPrefix  = "@_";
                sassPrefix = "$_";
            }
            var _left = string.Empty;
            var _top = string.Empty;
            
            if (isSass || isPhone)
            {
//                _left = left == 0 ? "0" : "0 " + "-{2}" + left.ToString() + unit;
//                _top = top == 0 ? "0" : "0 " + "-{2}" + top.ToString() + unit;
				  _left = left == 0 ? "0" : (0 - left).ToString() + unit;
                  _top = top == 0 ? "0" : (0 - top).ToString() + unit;
            }
            else {
                _left = left == 0 ? "0" : (0 - left).ToString() + unit;
                _top = top == 0 ? "0" : (0 - top).ToString() + unit;
            }
            var imgHeight = isPhone ? img.Height.ToString()  : img.Height.ToString()+unit;
            var imgWidth = isPhone ? img.Width.ToString() : img.Width.ToString() + unit;
            var str = "{0}" + GetCssName(imgInfo.Name) + "[height:{1}" + imgHeight  + ";width:{1}" + imgWidth  + ";" + "background-position:" + _left + " " + _top + ";]" + Environment.NewLine;
            if (isSass)
            {
                return String.Format(str, "@mixin ", sassPrefix, sassPrefix).Replace("[", "{").Replace("]", "}");
            }
            else 
            {
                return String.Format(str, ".", lessPrefix, lessPrefix).Replace("[", "{").Replace("]", "}");
            }
        }

        void SetBase64()
        {
            string base64Sass = string.Empty;
            string base64Css = string.Empty;
            BinaryFormatter binFormatter = new BinaryFormatter();
            ImageInfo imageInfo;
            int height, width;
            var isPhone = chkBoxPhone.Checked;
            var unit = "px";
            var sassPrefix = string.Empty;
            var lessPrefix = string.Empty;
            if (isPhone)
            {
                unit = "rem";
                lessPrefix = "@_";
                sassPrefix = "$_";
            }
            var _height = string.Empty;
            var _width = string.Empty;
            foreach (PictureBox pb in panelImages.Controls)
            {
                Bitmap bmp = new Bitmap(pb.Image, pb.Image.Width, pb.Image.Height);
                imageInfo = (ImageInfo)pb.Image.Tag;
                MemoryStream memStream = new MemoryStream();
                ImageFormat format = ImageFormat.Png;
                switch (Path.GetExtension(imageInfo.FileName))
                {
                    case "jpeg":
                        format = ImageFormat.Jpeg;
                        break;
                    case "jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case "png":
                        format = ImageFormat.Png;
                        break;
                    case "gif":
                        format = ImageFormat.Gif;
                        break;
                    default:
                        break;
                }
                bmp.Save(memStream, format);
                byte[] arr = new byte[memStream.Length];
                memStream.Position = 0;
                memStream.Read(arr, 0, (int)memStream.Length);
                memStream.Close();
                height = pb.Image.Height;
                //height = isPhone ? height / 2 : height;
                width = pb.Image.Width;
                //width = isPhone ? width / 2 : width;
                _height = height == 0 ? "0" : "{0}" + height.ToString() + unit;
                _width = width == 0 ? "0" : "{0}" + width.ToString() + unit;
                base64Sass += "@mixin ";
                base64Css += ".";
                var code = GetCssName(imageInfo.Name) + "[height:" + _height + ";width:" + _width + ";background:url(data:image/png;base64," + Convert.ToBase64String(arr) + ") no-repeat]" + Environment.NewLine;
                base64Sass += String.Format(code, sassPrefix).Replace("[", "{").Replace("]", "}");
                base64Css += String.Format(code, lessPrefix).Replace("[", "{").Replace("]", "}");
            }

            txtBase64Sass.Text = base64Sass;
            txtBase64Css.Text = base64Css;
        }

        string GetCssName(string imgName)
        {
            if (Char.IsNumber(imgName[0]))
            {
                return "_" + imgName;
            }
            return imgName;
        }

        public string GetImgName(Image img)
        {
            foreach (ImageInfo ii in _imgList)
            {
                if (ii.Image == img)
                {
                    return ii.Name;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// ����ͼƬ
        /// </summary>
        /// <param name="img">ͼƬ</param>
        /// <param name="left">���</param>
        /// <param name="top">�ϱ�</param>
        private void AddPictureBox(Image img, int left, int top)
        {
            PictureBox pb = new PictureBox();
            pb.Image = img;
            pb.Location = new System.Drawing.Point(left, top);
            pb.Cursor = Cursors.SizeAll;
            pb.BorderStyle =BorderStyle.FixedSingle ;
            pb.Name = "pb_" + left + "_" + top;
            pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            pb.Click += pb_Click;
            pb.MouseDown += pb_MouseDown;
            pb.MouseMove += pb_MouseMove;
            pb.MouseUp += pb_MouseUp;
            //pb.Paint += pb_Paint;
            panelImages.Controls.Add(pb);
            pb.Show();
        }

        void pb_Click(object sender, EventArgs e)
        {
            var p = (PictureBox)sender;
            p.Tag="1";
            p.Refresh();
            panelImages.Focus();
            _selectedPicture = p;
        }

        void pb_Paint(object sender, PaintEventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            if (p.Tag!=null && p.Tag.ToString() == "1") 
            {
                Pen pp = new Pen(Color.Blue);
                e.Graphics.DrawRectangle(pp, e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.X + e.ClipRectangle.Width - 1, e.ClipRectangle.Y + e.ClipRectangle.Height - 1);
                foreach (PictureBox pb in panelImages.Controls)
                {
                    if (pb != p)
                    {
                        pb.Tag = null;
                        pb.Refresh();
                    }
                }
            }
        }

        #region �϶�
        bool _isDragged = false;
        Point _dragStartLocation;
        void pb_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragged = false;
        }

        void pb_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragged)
            {
                PictureBox pb = sender as PictureBox;
                Point p = e.Location;
                int x = Math.Max(0, pb.Location.X + p.X - _dragStartLocation.X);
                int y = Math.Max(0, pb.Location.Y + p.Y - _dragStartLocation.Y);
                pb.Location = new Point(x, y);
                SetCssText();
                panelImages.ResumeLayout(false);
                panelImages.Refresh();
            }
        }

        void pb_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragged = true;
                _dragStartLocation = new Point(e.X, e.Y);
            }
            else
            {
                _isDragged = false;
            }
        }


        #endregion

        private void ButtonMakeBigImageCss_Click(object sender, EventArgs e)
        {
            panelImages.VerticalScroll.Value=0 ;
            panelImages.HorizontalScroll.Value = 0;
            if (_imgList == null || _imgList.Count < 2)
            {
                MessageBox.Show("��ѡ��������ͼƬ��");
                return;
            }

            DialogResult dr = folderBrowserDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string imgDir = folderBrowserDialog.SelectedPath;
                if (!Directory.Exists(imgDir))
                {
                    Directory.CreateDirectory(imgDir);
                }
                string imgPath = Path.Combine(imgDir, txtName.Text+"."+GetImgExt());
                if (File.Exists(imgPath))
                {
                    if (DialogResult.Yes !=
                        MessageBox.Show("ѡ���ļ������Ѵ���" + txtName.Text + "." + GetImgExt() + "������ִ�н������Ѵ����ļ����Ƿ������", "ѯ��"
                        , MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        return;
                    }
                }

                int maxWidth,maxHeight,minWidth,minHeight;
                maxWidth = maxHeight = minWidth = minHeight = 0;
                //ѭ����ȡ������ߺ��ϱ���С����
                //������Ԫ�ذ���0��0��Ϊ��׼��ͨ����С���Ͼ�����������ƽ�ƣ���ȡ������
                foreach (PictureBox pb in panelImages.Controls)
                {
                    if (panelImages.Controls.GetChildIndex(pb) == 0)
                    {
                        minWidth = pb.Location.X;
                        minHeight = pb.Location.Y;
                    }
                    minWidth = Math.Min(minWidth, pb.Location.X);
                    minHeight = Math.Min(minHeight, pb.Location.Y);
                    maxWidth = Math.Max(maxWidth, pb.Location.X + pb.Image.Width);
                    maxHeight = Math.Max(maxHeight, pb.Location.Y + pb.Image.Height);
                }
                Size imgSize = new Size(maxWidth, maxHeight);
                //var codeMime = string.Empty;
                using (Bitmap bigImg = new Bitmap(imgSize.Width-minWidth, imgSize.Height-minHeight, PixelFormat.Format32bppArgb))
                {
                    string imgType = GetImgExt();
                    ImageFormat format = ImageFormat.Png;
                    switch (imgType)
                    {
                        case "jpeg":
                            format = ImageFormat.Jpeg;
                            break;
                        case "jpg":
                            format = ImageFormat.Jpeg;
                            break;
                        case "png":
                            format = ImageFormat.Png;
                            break;
                        default:
                            break;
                    }
                    using (Graphics g = Graphics.FromImage(bigImg))
                    {
                        //���ø�������ֵ�� 
                        g.InterpolationMode = InterpolationMode.High;
                        //���ø�����,���ٶȳ���ƽ���� 
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        //��ջ�������͸������ɫ��� 
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        if ((format == ImageFormat.Jpeg)) g.Clear(Color.White);
                        else g.Clear(Color.Transparent);
                        
                        SetCssText();
                        SetBase64();
                        var sprite = new SpriteFile() { 
                            CssFileName = txtDir.Text,
                            ImageName = txtName.Text,
                            SpriteList = new List<Sprite>(), 
                            IsPhone = chkBoxPhone.Checked,
                            SpriteImgFileType = comboBoxImgType.Text
                        };                        
                        try
                        {
                            foreach (PictureBox pb in panelImages.Controls)
                            {
                                var img = (ImageInfo)pb.Image.Tag;
                                var path = img.FileName;
                                Sprite s = new Sprite() { LocationY = pb.Location.Y, LocationX = pb.Location.X, Path = Path.GetFileName(path) };
                                sprite.SpriteList.Add(s);
                                g.DrawImage(pb.Image, pb.Location.X - minWidth, pb.Location.Y - minHeight, pb.Image.Width, pb.Image.Height);
                                if (Path.GetDirectoryName(path) != folderBrowserDialog.SelectedPath)
                                {
                                    File.Copy(path, folderBrowserDialog.SelectedPath + "\\" + Path.GetFileName(path), false);
                                }
                            }
                            XmlSerializer.SaveToXml(folderBrowserDialog.SelectedPath + "\\" + txtName.Text + ".sprite", sprite);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message,"��ʾ",MessageBoxButtons.OK,MessageBoxIcon.Error);
                            return;
                        }
                    }
                    try
                    {
                        //����ͼƬ
                        bigImg.Save(imgPath, format);
                        MessageBox.Show("ͼƬ���ɳɹ���");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message+"ͼƬ����ʧ�ܣ��������ļ����ܱ���������ռ�ã��뻻���ļ�����");
                    }
                }
            }
        }


        public bool IsImgExists(string fileName)
        {
            foreach (ImageInfo  ii in _imgList)
            {
                if (string.Compare(ii.FileName,fileName,true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void txtSass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) { txtSass.SelectAll(); }   
        }


        private void txtDir_TextChanged(object sender, EventArgs e)
        {
            SetCssText();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            SetCssText();
        }

        //Сͼ���ŵ��
        private void buttonHRange_Click(object sender, EventArgs e)
        {
            if (!AssertFiles()) return;
            panelImages.Controls.Clear();
            int left = 0;
            int top = 0;
            foreach (ImageInfo ii in _imgList)
            {
                Image img = ii.Image;
                AddPictureBox(img, left, top);
                left += img.Width;
            }

            panelImages.ResumeLayout(false);
            SetCssText();
        }

        List<PictureBox> _list ;
        private void chkBoxPhone_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxPhone.Checked)
            {
                panelPhone.Visible = true;
                _list = new List<PictureBox>();
                foreach (PictureBox pb in panelImages.Controls)
                {
                    _list.Add(pb);
                }
                //����Y������
                for (int i = 0; i < _list.Count; i++)
                {
                    for (int j = 0; j < _list.Count; j++)
                    {
                        if (_list[i].Location.Y < _list[j].Location.Y)
                        {
                            var temp = _list[i];
                            _list[i] = _list[j];
                            _list[j] = temp;
                        }
                    }
                }
                var left = 0;
                var preY = 0;
                var edgeSize = GetEdgeSize(_list);
                for (var i = 0; i < _list.Count; i++) {
                    var item=_list[i];
                    var preItem=i>0?_list[i-1]:null;
                    if (edgeSize.MinHeight != item.Location.Y)
                    {
                        if (preY == 0) {
                            preY = preItem.Location.Y;
                        }
                        if (preY + preItem.Height - item.Location.Y == 2)
                        {
                            preY = item.Location.Y;
                            var _left = item.Location.Y + left;
                            item.Location = new Point(item.Location.X, _left);
                        }
                    }
                    left++;
                }
                //����X����
                for (int i = 0; i < _list.Count; i++)
                {
                    for (int j = 0; j < _list.Count; j++)
                    {
                        if (_list[i].Location.X < _list[j].Location.X)
                        {
                            var temp = _list[i];
                            _list[i] = _list[j];
                            _list[j] = temp;
                        }
                    }
                }
                var top = 0;
                var preX = 0;
                for (var i = 0; i < _list.Count; i++)
                {
                    var item = _list[i];
                    var preItem = i > 0 ? _list[i - 1] : null;
                    if (edgeSize.MinWidth != item.Location.X)
                    {
                        if (preX == 0)
                        {
                            preX = preItem.Location.X;
                        }
                        if (preX + preItem.Width - item.Location.X == 2)
                        {
                            preX = item.Location.X;
                            var _top = item.Location.X + top;
                            item.Location = new Point(_top, item.Location.Y);
                        }
                    }
                    top++;
                }
            }
            else
            {
                panelPhone.Visible = false;
            }
            if (sender != null) {
                SetCssText();
                SetBase64();
            }
        }

        private void SetMargin(PictureBox pictureBox)
        {
            var locationPiont = new Point(pictureBox.Location.X, pictureBox.Location.Y);
            foreach (PictureBox pb in _list)
            {                if (pictureBox.Location.X - (pb.Location.X + pb.Width) == -2 && pictureBox.Location.Y == pb.Location.Y)  
                {
                    
                    if (locationPiont.X > 0)
                    {
                        locationPiont.X++;
                    }
                }
                if (pictureBox.Location.Y- (pb.Location.Y + pb.Height) == -2 && pictureBox.Location.X == pb.Location.X) {
                    if (locationPiont.Y > 0)
                    {
                        locationPiont.Y++;
                    }
                } 
            }
            pictureBox.Location = locationPiont;
        }

        private void btn_Click(object sender, EventArgs e)
        {
            AboutUs a=new AboutUs();
            a.ShowDialog();
        }

        private void txtCss_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) { txtCss.SelectAll(); }  
        }

        private void txtBase64Sass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) { txtBase64Sass.SelectAll(); }  
        }

        private void txtBase64Css_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) { txtBase64Css.SelectAll(); }  
        }

        private void comboBoxImgType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCssText();
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.cnblogs.com/wang4517/archive/2015/05/19/4514862.html");
        }

        private void linkLabelSass_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://csssprite.herokuapp.com/sassVar");
        }

        private void linkLabelLess_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://csssprite.herokuapp.com/lessVar");
        }
        
    }
}