
function RunNotBundle() {
    var variable = "alert from not bundled";
    alert(variable);
    //variables will be renamed, commentaries will be gone on RELEASE
}

RunNotBundle();