if ('objectFit' in document.documentElement.style === false) {
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('img[data-object-fit]').forEach(function (image) {
            (image.runtimeStyle || image.style).background = "url(\"".concat(image.src, "\") no-repeat 50%/").concat(image.currentStyle ? image.currentStyle['object-fit'] : image.getAttribute('data-object-fit'));
            image.src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='".concat(image.width, "' height='").concat(image.height, "'%3E%3C/svg%3E");
        });
    });
}