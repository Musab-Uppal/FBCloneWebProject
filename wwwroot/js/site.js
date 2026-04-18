document.addEventListener("DOMContentLoaded", () => {
  const pageLoader = document.getElementById("globalPageLoader");
  const networkLoader = document.createElement("div");
  networkLoader.className = "network-loader";
  document.body.appendChild(networkLoader);

  let activeNetworkRequests = 0;

  const setNetworkLoader = (isVisible) => {
    if (isVisible) {
      networkLoader.classList.add("visible");
    } else {
      networkLoader.classList.remove("visible");
    }
  };

  const beginNetworkRequest = () => {
    activeNetworkRequests += 1;
    setNetworkLoader(true);
  };

  const endNetworkRequest = () => {
    activeNetworkRequests = Math.max(0, activeNetworkRequests - 1);
    if (activeNetworkRequests === 0) {
      setNetworkLoader(false);
    }
  };

  // Keep loader subtle: quick entrance, then fade out once the UI is ready.
  if (pageLoader) {
    window.setTimeout(() => pageLoader.classList.add("hidden"), 180);
  }

  const revealTargets = document.querySelectorAll(
    ".card, .list-group-item, .quick-link, .notification-item, .alert, .table, .pagination, .modal-content",
  );

  revealTargets.forEach((el, index) => {
    el.classList.add("reveal-in");
    el.style.transitionDelay = `${Math.min(index * 14, 180)}ms`;
  });

  if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("revealed");
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.1 },
    );

    revealTargets.forEach((el) => observer.observe(el));
  } else {
    revealTargets.forEach((el) => el.classList.add("revealed"));
  }

  document.addEventListener("click", (event) => {
    const anchor = event.target.closest("a[href]");
    if (!anchor || !pageLoader) {
      return;
    }

    const href = anchor.getAttribute("href");
    const isBootstrapTrigger =
      anchor.hasAttribute("data-bs-toggle") ||
      anchor.getAttribute("role") === "button";

    if (
      !href ||
      href.startsWith("#") ||
      href.startsWith("javascript:") ||
      anchor.target === "_blank" ||
      anchor.hasAttribute("download") ||
      isBootstrapTrigger
    ) {
      return;
    }

    pageLoader.classList.remove("hidden");
  });

  document.addEventListener("click", (event) => {
    const button = event.target.closest(".btn");
    if (!button) {
      return;
    }

    button.classList.remove("tap-pop");
    window.requestAnimationFrame(() => {
      button.classList.add("tap-pop");
    });
  });

  if (typeof window.fetch === "function") {
    const originalFetch = window.fetch.bind(window);
    window.fetch = (...args) => {
      beginNetworkRequest();
      return originalFetch(...args).finally(() => {
        endNetworkRequest();
      });
    };
  }

  if (window.jQuery) {
    window
      .jQuery(document)
      .ajaxSend(() => {
        beginNetworkRequest();
      })
      .ajaxComplete(() => {
        endNetworkRequest();
      });
  }
});
