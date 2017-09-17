// image-zoom.js

// minimize image on click
var image_popup = document.getElementById('image-popup');
var image_popup_link = document.getElementById('image-popup-link');
var image_popup_img = document.getElementById('image-popup-img');
var image_popup_figure = document.getElementById('image-popup-figure');
var image_popup_caption = document.getElementById('image-popup-caption');
image_popup_figure.onclick = function() {
  image_popup.style.display='none';
}

//
{
  function img_resize_event() {
    var w = Math.max(document.documentElement.clientWidth, window.innerWidth || 0);
    var h = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);
    if (image_popup_img.naturalWidth > w || image_popup_img.naturalHeight > h) {
      image_popup_link.style.display = 'block';
    } else {
      image_popup_link.style.display = 'none';
    }
  }

  window.addEventListener('resize', img_resize_event);

  function img_click_event() {
    image_popup_img.src = this.src;
    image_popup.style.display = 'block';
    image_popup_link.href = this.src;

    var w = Math.max(document.documentElement.clientWidth, window.innerWidth || 0);
    var h = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);
    if (this.naturalWidth > w || this.naturalHeight > h) {
      image_popup_link.style.display = 'block';
    } else {
      image_popup_link.style.display = 'none';
    }
  }

  var zoom_images = document.getElementsByClassName("image-zoom");
  for( var i=0; i < zoom_images.length; ++i) {
    zoom_images[i].children[0].onclick = img_click_event;
  }
}
