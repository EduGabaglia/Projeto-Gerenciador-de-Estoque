const senha = document.getElementById("Senha");
const togglePassword = document.getElementById("toggle_password");
const eyeIcon = togglePassword.querySelector("i");

togglePassword.addEventListener("click", function () {

    eyeIcon.style.opacity = "0";
    eyeIcon.style.transform = "scale(0.7)";

    setTimeout(function () {

        if (senha.type === "password") {
            senha.type = "text";
            eyeIcon.classList.remove("bi-eye-slash");
            eyeIcon.classList.add("bi-eye");
        } else {
            senha.type = "password";
            eyeIcon.classList.remove("bi-eye");
            eyeIcon.classList.add("bi-eye-slash");
        }

        eyeIcon.style.opacity = "1";
        eyeIcon.style.transform = "scale(1)";

    }, 150);

});