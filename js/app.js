function highlightCode() {
    var markdownElement = document.getElementById("markdown");

    markdownElement.style.display = "none";

    var pres = document.querySelectorAll("pre>code");
    for (var i = 0; i < pres.length; i++) {
        hljs.highlightBlock(pres[i]);
    }

    markdownElement.style = "display:block; -webkit-animation: fadeIn 1s;animation: fadeIn 1s;";
}

var getTheme = function () {
    return JSON.parse(localStorage.getItem("darkmode") ?? userSystemIsDarkMode()) ? "dark" : "light";
}

var loadTheme = function () {
    var theme = getTheme();
    document.documentElement.setAttribute('data-theme', theme);
}

var userSystemIsDarkMode = function () {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

var toggleTheme = function () {
    var lsValue = JSON.parse(localStorage.getItem("darkmode") ?? userSystemIsDarkMode());
    var theme = !lsValue ? "dark" : "light";

    localStorage.setItem("darkmode", !lsValue);
    document.documentElement.setAttribute('data-theme', theme);

    return theme;
}