let names = ["Orelsan", "Angèle", "Damso", "Nekfeu"];

function sendit() {
    document.getElementById("search-form-input").placeholder = "Essayez avec : "+ names[Math.floor(Math.random() * 4)];
    // Call sendit() the next time, repeating
    setTimeout(sendit, 2500);
}

sendit();