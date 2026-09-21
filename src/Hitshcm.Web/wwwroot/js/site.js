(() => {
  const closeMenus = (except) => {
    document.querySelectorAll("details.user-menu[open]").forEach((menu) => {
      if (menu !== except) {
        menu.removeAttribute("open");
      }
    });
  };

  document.addEventListener("click", (event) => {
    const openMenu = event.target.closest("details.user-menu");
    closeMenus(openMenu);
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      closeMenus();
    }
  });
})();
