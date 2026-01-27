window.dainikiQuill = window.dainikiQuill || {
    editors: {},
    initEditor: function (editorId, initialHtml) {
        if (!editorId) {
            return;
        }

        if (this.editors[editorId]) {
            if (initialHtml) {
                this.setHtml(editorId, initialHtml);
            }
            return;
        }

        if (typeof Quill === "undefined") {
            console.warn("Quill not loaded. Ensure quill.min.js is available locally.");
            return;
        }

        var container = document.getElementById(editorId);
        if (!container) {
            return;
        }

        var toolbarOptions = [
            [{ header: [1, 2, 3, false] }],
            ["bold", "italic", "underline", "strike"],
            [{ list: "ordered" }, { list: "bullet" }],
            ["link"],
            ["clean"]
        ];

        var editor = new Quill(container, {
            theme: "snow",
            modules: {
                toolbar: toolbarOptions
            }
        });

        if (initialHtml) {
            editor.clipboard.dangerouslyPasteHTML(initialHtml);
        }

        this.editors[editorId] = editor;
    },
    getHtml: function (editorId) {
        var editor = this.editors[editorId];
        if (!editor) {
            return "";
        }

        return editor.root.innerHTML;
    },
    setHtml: function (editorId, html) {
        var editor = this.editors[editorId];
        if (!editor) {
            return;
        }

        editor.clipboard.dangerouslyPasteHTML(html || "");
    }
};
