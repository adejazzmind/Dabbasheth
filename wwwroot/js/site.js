/**
 * DABBASHETH SECURE WALLET - PREMIUM INTERACTION ENGINE
 * v3.5 - Full Synchronization Fix
 */

// --- 1. THEME ENGINE (Self-Executing) ---
(function () {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-bs-theme', savedTheme);

    document.addEventListener('DOMContentLoaded', () => {
        const icon = document.getElementById('themeIcon');
        if (savedTheme === 'dark' && icon) {
            icon.classList.replace('bi-brightness-high-fill', 'bi-moon-stars-fill');
        }
    });
})();

function toggleTheme() {
    const htmlElement = document.documentElement;
    const icon = document.getElementById('themeIcon');
    const currentTheme = htmlElement.getAttribute('data-bs-theme');

    if (currentTheme === 'dark') {
        htmlElement.setAttribute('data-bs-theme', 'light');
        icon.classList.replace('bi-moon-stars-fill', 'bi-brightness-high-fill');
        localStorage.setItem('theme', 'light');
    } else {
        htmlElement.setAttribute('data-bs-theme', 'dark');
        icon.classList.replace('bi-brightness-high-fill', 'bi-moon-stars-fill');
        localStorage.setItem('theme', 'dark');
    }
}

// --- 2. CORE TRANSACTION & ALERT LOGIC ---
/**
 * Handles Success Alerts & Sounds for all transactions
 * @param {string} formId - The ID of the form to submit after confirmation
 */
function handleTransaction(formId) {
    // 1. Play Premium Notification Sound
    const audio = new Audio('https://www.soundjay.com/buttons/sounds/button-3.mp3');
    audio.play().catch(e => console.log("Audio play blocked by browser."));

    // 2. Trigger World-Class Success Alert
    Swal.fire({
        title: 'Transaction Successful!',
        text: 'Your Dabbasheth wallet has been updated.',
        icon: 'success',
        confirmButtonText: 'View Receipt',
        confirmButtonColor: '#00D09C',
        backdrop: `rgba(0,0,0,0.5)`,
        allowOutsideClick: false,
        customClass: {
            popup: 'rounded-4 shadow-lg border-0'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            // 3. Submit the specific form only AFTER the user clicks "View Receipt"
            const targetForm = document.getElementById(formId);
            if (targetForm) {
                targetForm.submit();
            }
        }
    });
}

// --- 3. UI ENHANCEMENTS ---
$(document).ready(function () {
    // Action Icon Hover Animations
    $(".action-icon").hover(
        function () {
            $(this).css({ 'transform': 'scale(1.08) translateY(-3px)', 'transition': 'all 0.3s ease' });
        },
        function () {
            $(this).css({ 'transform': 'scale(1) translateY(0)' });
        }
    );

    // Auto-dismiss standard alerts
    setTimeout(function () {
        $(".alert").fadeOut('slow');
    }, 5000);
});