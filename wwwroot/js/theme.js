document.addEventListener("DOMContentLoaded", () => {
    const backToTop = document.getElementById("backToTop");
    const header = document.querySelector(".header");

    if (backToTop) {
        const toggleBackToTop = () => {
            if (window.scrollY > 280) {
                backToTop.classList.add("visible");
            } else {
                backToTop.classList.remove("visible");
            }
        };

        toggleBackToTop();
        window.addEventListener("scroll", toggleBackToTop, { passive: true });
        backToTop.addEventListener("click", () => window.scrollTo({ top: 0, behavior: "smooth" }));
    }

    if (header) {
        const updateHeader = () => {
            if (window.scrollY > 24) {
                header.style.backgroundColor = "rgba(246, 250, 255, 0.92)";
            } else {
                header.style.backgroundColor = "rgba(246, 250, 255, 0.83)";
            }
        };

        updateHeader();
        window.addEventListener("scroll", updateHeader, { passive: true });
    }

    const searchInput = document.getElementById("globalSearch");
    const searchResults = document.getElementById("quickSearchResults");

    if (searchInput && searchResults) {
        let searchTimeout;

        searchInput.addEventListener("input", function () {
            clearTimeout(searchTimeout);
            const query = this.value.trim();

            if (query.length < 2) {
                searchResults.style.display = "none";
                return;
            }

            searchTimeout = setTimeout(async () => {
                try {
                    searchResults.innerHTML = `
                        <div class="p-3 d-flex align-items-center gap-2 text-muted">
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                            Searching...
                        </div>`;
                    searchResults.style.display = "block";

                    const response = await fetch(`/Search/QuickSearch?q=${encodeURIComponent(query)}`);
                    const data = await response.json();

                    if (!data.success) {
                        searchResults.style.display = "none";
                        return;
                    }

                    let html = "<div class='search-results-content p-3'>";

                    if (data.users.length > 0) {
                        html += "<div class='search-category mb-2'><h6 class='text-uppercase small text-muted'>People</h6></div>";
                        data.users.forEach(user => {
                            html += `
                                <a href="/Profile/View/${user.id}" class="search-result-item d-flex align-items-center gap-2 p-2 rounded text-decoration-none mb-1">
                                    <div class="result-avatar"><i class="bi bi-person-circle"></i></div>
                                    <div class="result-info flex-grow-1">
                                        <strong class="d-block">${user.name}</strong>
                                        <small class="text-muted">${user.email}</small>
                                    </div>
                                </a>`;
                        });
                    }

                    if (data.posts.length > 0) {
                        html += "<div class='search-category mb-2 mt-2'><h6 class='text-uppercase small text-muted'>Posts</h6></div>";
                        data.posts.forEach(post => {
                            html += `
                                <a href="/Home/Feed#post-${post.id}" class="search-result-item d-flex align-items-center gap-2 p-2 rounded text-decoration-none mb-1">
                                    <div class="result-avatar"><i class="bi bi-file-text"></i></div>
                                    <div class="result-info flex-grow-1">
                                        <strong class="d-block">${post.userName}</strong>
                                        <small class="text-muted">${post.content}</small>
                                    </div>
                                </a>`;
                        });
                    }

                    if (data.groups.length > 0) {
                        html += "<div class='search-category mb-2 mt-2'><h6 class='text-uppercase small text-muted'>Groups</h6></div>";
                        data.groups.forEach(group => {
                            html += `
                                <a href="/Groups/Details/${group.id}" class="search-result-item d-flex align-items-center gap-2 p-2 rounded text-decoration-none mb-1">
                                    <div class="result-avatar"><i class="bi bi-people"></i></div>
                                    <div class="result-info flex-grow-1">
                                        <strong class="d-block">${group.name}</strong>
                                        <small class="text-muted">${group.memberCount} members</small>
                                    </div>
                                </a>`;
                        });
                    }

                    if (data.users.length === 0 && data.posts.length === 0 && data.groups.length === 0) {
                        html += "<div class='text-muted small px-2 py-3'>No quick matches found</div>";
                    }

                    html += "</div>";
                    searchResults.innerHTML = html;
                    searchResults.style.display = "block";
                } catch (error) {
                    console.error("Search error:", error);
                    searchResults.style.display = "none";
                }
            }, 260);
        });

        document.addEventListener("click", event => {
            if (!searchInput.contains(event.target) && !searchResults.contains(event.target)) {
                searchResults.style.display = "none";
            }
        });

        document.addEventListener("keydown", event => {
            if (event.key === "Escape") {
                searchResults.style.display = "none";
            }
        });
    }

    const fileInput = document.getElementById("postImage");
    if (fileInput) {
        const optionLabel = fileInput.previousElementSibling;
        fileInput.addEventListener("change", function () {
            if (this.files.length > 0 && optionLabel) {
                optionLabel.innerHTML = `<i class="bi bi-check-circle"></i> ${this.files[0].name}`;
            }
        });
    }

    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(el => new bootstrap.Tooltip(el));

    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(el => new bootstrap.Popover(el));
});

window.toggleChat = function () {
    const chatContainer = document.getElementById("chatContainer");
    if (chatContainer) {
        chatContainer.classList.toggle("open");
    }
};
