function UserInputEvents() {
  document.addEventListener("DOMContentLoaded", () => {
    const menuItems = document.querySelectorAll(".menu-item");
    const dropdownItems = document.querySelectorAll(".dropdown-item");

    menuItems.forEach(item => {
      item.addEventListener("click", e => {
        if (item.classList.contains("has-dropdown")) {
          e.stopPropagation();
          menuItems.forEach(i => {
            if (i !== item) i.classList.remove("active");
          });
          item.classList.toggle("active");
        } else {
          handleAction(item.dataset.action);
        }
      });
    });

    dropdownItems.forEach(drop => {
      drop.addEventListener("click", e => {
        e.stopPropagation();
        handleAction(drop.dataset.action);
        menuItems.forEach(i => i.classList.remove("active"));
      });
    });

    document.addEventListener("click", () => {
      menuItems.forEach(i => i.classList.remove("active"));
    });
  });
}

function handleAction(action) {
  alert("Action: " + action);
  switch (action) {
    case "loadDashboard": loadDashboard(); break;
    case "loadLogs": loadLogs(); break;
    case "loadNotes": loadNotes(); break;
    case "loadAbout": loadAbout(); break;
    default: console.warn("Unknown action:", action); break;
  }

}
