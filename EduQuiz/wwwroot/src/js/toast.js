const toast = document.querySelector("#toast");
const toastTimer = document.querySelector("#timer");
const closeToastBtn = document.querySelector("#toast-close");
const toastTitle = document.querySelector("#toast-title");
const toastDescription = document.querySelector("#toast-description");

let countdown;

const closeToast = () => {
    toast.style.animation = "close 0.3s cubic-bezier(.87,-1,.57,.97) forwards";
    toastTimer.classList.remove("timer-animation");
    clearTimeout(countdown);
};

const openToast = (type, title, description, time = 5000) => {
    toast.classList = [type];
    toastTitle.textContent = title;
    toastDescription.textContent = description;
    toast.style.animation = "open 0.3s cubic-bezier(.47,.02,.44,2) forwards";

    // Set the custom property --toast-timer to the provided time
    toastTimer.style.setProperty('--toast-timer', `${time / 1000}s`);

    // Add timer animation class
    toastTimer.classList.add("timer-animation");

    clearTimeout(countdown);
    countdown = setTimeout(() => {
        closeToast();
    }, time);
};

closeToastBtn.addEventListener("click", closeToast);

// Example usage:
// openToast('info', 'Success', 'Your operation was successful!', 3000);

//success, warning, error, info
