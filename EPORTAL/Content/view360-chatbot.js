/* ===================================================================
 *  V360 Chatbot widget (user-facing).
 *
 *  Phu thuoc tu Details.cshtml (expose qua window):
 *    - window.V360CB_COLLECTION_ID  : string
 *    - window.V360CB_ASK_URL        : string
 *    - window.V360CB_GREETING       : optional string (greeting line khac default)
 *    - window.__v360CurrentScene()  : () => { uuid, name } - state scene hien tai
 *    - window.kuulaSend(action, data) : function de trigger Kuula iframe (load scene)
 *
 *  Tat ca optional; widget se tu disable graceful neu thieu.
 * =================================================================== */
(function () {
    'use strict';
    if (window.__v360cbBooted) return;
    window.__v360cbBooted = true;

    var stage = document.querySelector('.v360-viewer__stage');
    if (!stage) {
        console.warn('[v360cb] no .v360-viewer__stage container - chatbot widget khong mount');
        return;
    }
    var COLLECTION_ID = window.V360CB_COLLECTION_ID || '';
    var ASK_URL       = window.V360CB_ASK_URL || '';
    if (!COLLECTION_ID || !ASK_URL) {
        console.warn('[v360cb] V360CB_COLLECTION_ID hoac V360CB_ASK_URL khong set');
        return;
    }

    // ====== Session state ======
    function genGuid() {
        if (window.crypto && crypto.randomUUID) return crypto.randomUUID();
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    var SESSION_GUID = sessionStorage.getItem('v360cb_session') || genGuid();
    sessionStorage.setItem('v360cb_session', SESSION_GUID);
    var history = [];        // [{role, content}]
    var busy = false;
    var greeted = false;

    // ====== Build DOM ======
    var fab = document.createElement('button');
    fab.type = 'button';
    fab.className = 'v360cb-fab';
    fab.title = 'Hướng dẫn viên AI';
    fab.innerHTML = '<i class="fa fa-comments"></i><span class="v360cb-fab__badge">AI</span>';
    stage.appendChild(fab);

    var panel = document.createElement('aside');
    panel.className = 'v360cb';
    panel.innerHTML =
        '<span class="v360cb__corner-tl"></span>' +
        '<span class="v360cb__corner-br"></span>' +
        '<div class="v360cb__header">' +
            '<div class="v360cb__icon-wrap">' +
                '<div class="v360cb__icon"><i class="fa fa-robot"></i></div>' +
            '</div>' +
            '<div class="v360cb__title">' +
                '<div class="v360cb__title-row">' +
                    '<span>Trợ lý ảo</span>' +
                    '<span class="v360cb__experiment"><i class="fa fa-flask"></i> Hệ thống thử nghiệm</span>' +
                '</div>' +
                '<div class="v360cb__subtitle">P.CNTT&CĐS - HPDQ</div>' +
            '</div>' +
            '<button type="button" class="v360cb__close" title="Đóng"><i class="fa fa-times"></i></button>' +
        '</div>' +
        // Mode selector: text / tts (text-in voice-out) / voice (full voice)
        '<div class="v360cb__modes" role="tablist">' +
            '<button type="button" class="v360cb__mode is-active" data-mode="text" title="Văn bản → Văn bản">' +
                '<i class="fa fa-keyboard"></i><span>Text</span></button>' +
            '<button type="button" class="v360cb__mode" data-mode="tts" title="Văn bản → Giọng nói (đọc to)">' +
                '<i class="fa fa-volume-up"></i><span>Đọc</span></button>' +
            '<button type="button" class="v360cb__mode" data-mode="voice" title="Hội thoại giọng nói">' +
                '<i class="fa fa-microphone-alt"></i><span>Voice</span></button>' +
        '</div>' +
        '<div class="v360cb__body" id="v360cb-body"></div>' +
        '<div class="v360cb__voice-status" id="v360cb-vstat" style="display:none"></div>' +
        '<div class="v360cb__input-wrap">' +
            '<input type="text" class="v360cb__input" id="v360cb-input" ' +
                   'placeholder="// Nhập câu hỏi..." maxlength="500" />' +
            '<button type="button" class="v360cb__send" id="v360cb-send" title="Gửi (Enter)">' +
                '<i class="fa fa-paper-plane"></i></button>' +
        '</div>';
    stage.appendChild(panel);

    // Track minimap minimized state -> set class .cb-mm-mini tren stage.
    // De CSS reposition FAB + minimap icon theo state.
    var minimapEl = stage.querySelector('.v360-minimap');
    if (minimapEl) {
        var syncMmState = function () {
            stage.classList.toggle('cb-mm-mini', minimapEl.classList.contains('is-minimized'));
        };
        try {
            new MutationObserver(syncMmState)
                .observe(minimapEl, { attributes: true, attributeFilter: ['class'] });
        } catch (_) {}
        syncMmState();
    }

    var bodyEl   = panel.querySelector('#v360cb-body');
    var inputEl  = panel.querySelector('#v360cb-input');
    var sendBtn  = panel.querySelector('#v360cb-send');
    var closeBtn = panel.querySelector('.v360cb__close');

    // ====== Open/close - dung class swap tren stage de CSS transition swap minimap <-> panel ======
    function triggerLeafletRelayout() {
        // Goi _v360ApplyMinZoomAndFit SAU khi CSS transition (.35s) ket thuc.
        // KHONG goi som hon (vd 50ms) vi luc do container van dang resize, Leaflet
        // chia cho 0/transitional size -> NaN -> SVG transform error "scale(NaN)".
        var apply = function () {
            try {
                var mm = document.getElementById('v360-leaflet-map');
                if (!mm || mm.offsetWidth < 30 || mm.offsetHeight < 30) return;
                if (window._v360ApplyMinZoomAndFit) window._v360ApplyMinZoomAndFit();
            } catch (_) {}
        };
        setTimeout(apply, 400);  // sau CSS transition (.35s + buffer 50ms)
    }
    function openPanel() {
        stage.classList.add('cb-open');
        sessionStorage.setItem('v360cb_open', '1');
        setTimeout(function () { inputEl.focus(); }, 200);
        if (!greeted) greet();
        triggerLeafletRelayout();
    }
    function closePanel() {
        stage.classList.remove('cb-open');
        sessionStorage.setItem('v360cb_open', '0');
        triggerLeafletRelayout();
    }
    fab.addEventListener('click', openPanel);
    closeBtn.addEventListener('click', closePanel);
    // Restore state
    if (sessionStorage.getItem('v360cb_open') === '1') {
        setTimeout(openPanel, 200);
    }

    // ====== Render helpers ======
    function escHtml(s) {
        return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }
    function appendMsg(role, text, opts) {
        opts = opts || {};
        var div = document.createElement('div');
        var cls = role === 'user' ? 'v360cb__msg v360cb__msg--user'
                : (role === 'error' ? 'v360cb__msg v360cb__msg--error'
                                    : 'v360cb__msg v360cb__msg--bot');
        div.className = cls;
        // Basic newline-to-br; no markdown parsing for MVP
        var html = escHtml(text).replace(/\n/g, '<br>');
        if (opts.navTo) {
            html += '<div class="v360cb__nav"><i class="fa fa-location-arrow"></i> ' +
                    'Đang đưa bạn tới: ' + escHtml(opts.navName || opts.navTo) + '</div>';
        }
        div.innerHTML = html;
        bodyEl.appendChild(div);
        bodyEl.scrollTop = bodyEl.scrollHeight;
        return div;
    }
    function appendTyping() {
        var div = document.createElement('div');
        div.className = 'v360cb__typing';
        div.id = 'v360cb-typing';
        div.innerHTML = '<span></span><span></span><span></span>';
        bodyEl.appendChild(div);
        bodyEl.scrollTop = bodyEl.scrollHeight;
        return div;
    }
    function removeTyping() {
        var el = document.getElementById('v360cb-typing');
        if (el) el.remove();
    }

    // ====== Greeting ======
    function currentScene() {
        try {
            if (typeof window.__v360CurrentScene === 'function') return window.__v360CurrentScene();
        } catch (e) { /* ignore */ }
        return { uuid: null, name: null };
    }
    function greet() {
        greeted = true;
        var sc = currentScene();
        var msg = window.V360CB_GREETING ||
            (sc.name
                ? 'Xin chào! Tôi là hướng dẫn viên ảo của KLH HPDQ. Bạn đang xem ' + sc.name +
                  '. Hỏi tôi bất cứ điều gì về khu vực này hoặc tour!'
                : 'Xin chào! Tôi là hướng dẫn viên ảo của KLH HPDQ. Hỏi tôi bất cứ điều gì về tour!');
        appendMsg('bot', msg);
        // 3 cau hoi goi y mac dinh khi mo chatbot
        var greetSuggestions = sc.name
            ? ['Giới thiệu về ' + sc.name, 'Tour này có những khu nào?', 'Khu vực nổi bật?']
            : ['Giới thiệu về KLH HPDQ', 'Tour này có những khu nào?', 'Khu vực nổi bật?'];
        renderSuggestions(greetSuggestions);
    }

    // ===== Suggestion chips =====
    // Render 3 chip duoi bubble bot moi nhat. Click -> set input + send.
    // Khi user send cau moi, suggestions cu se bi clear truoc.
    function renderSuggestions(suggestions) {
        clearSuggestions();
        if (!suggestions || !suggestions.length) return;
        var wrap = document.createElement('div');
        wrap.className = 'v360cb__suggestions';
        wrap.id = 'v360cb-suggestions';
        suggestions.forEach(function (q) {
            if (!q) return;
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'v360cb__suggest';
            btn.textContent = q;
            btn.addEventListener('click', function () {
                if (busy) return;
                inputEl.value = q;
                send();
            });
            wrap.appendChild(btn);
        });
        bodyEl.appendChild(wrap);
        bodyEl.scrollTop = bodyEl.scrollHeight;
    }
    function clearSuggestions() {
        var old = document.getElementById('v360cb-suggestions');
        if (old) old.remove();
    }

    // ====== Send ======
    function setBusy(b) {
        busy = b;
        inputEl.disabled = b;
        sendBtn.disabled = b;
    }
    function tryNavigate(uuid, displayName) {
        // kuulaSend('load') needs Kuula iframe post id (numeric), NOT uuid.
        // Reverse lookup via window.__v360GetIdToUuid() - getter de tranh stale ref.
        // Retry tu tu vi iframe co the chua frameloaded khi nguoi dung hoi cau dau.
        var tries = 0;
        function attempt() {
            tries++;
            try {
                if (typeof window.kuulaSend === 'function') {
                    var map = typeof window.__v360GetIdToUuid === 'function'
                            ? window.__v360GetIdToUuid()
                            : (window.__v360IdToUuid || {});
                    var target = String(uuid || '').toLowerCase().replace(/-/g, '');
                    var postId = null;
                    for (var pid in map) {
                        var v = String(map[pid] || '').toLowerCase();
                        if (v === String(uuid).toLowerCase() || v.replace(/-/g, '') === target) {
                            postId = pid; break;
                        }
                    }
                    if (postId) {
                        console.log('[v360cb] navigate -> postId=' + postId + ' (uuid=' + uuid + ')');
                        window.kuulaSend('load', { id: postId });
                        return true;
                    }
                    if (tries === 1) {
                        console.warn('[v360cb] uuid ' + uuid + ' khong match trong idToUuid (' +
                                     Object.keys(map).length + ' entries) - thu lai...');
                    }
                }
            } catch (e) { console.warn('[v360cb] nav err', e); }
            if (tries < 8) setTimeout(attempt, 600);
            else console.error('[v360cb] navigate FAIL sau 8 lan thu - uuid ' + uuid);
            return false;
        }
        attempt();
    }

    // ===== Streaming reply rendering =====
    // Tao bubble bot moi (rong), tra ve API gan content + finalize.
    function createStreamingBubble() {
        var div = document.createElement('div');
        div.className = 'v360cb__msg v360cb__msg--bot v360cb__msg--streaming';
        div.innerHTML = '<span class="v360cb__text"></span><span class="v360cb__cursor"></span>';
        bodyEl.appendChild(div);
        bodyEl.scrollTop = bodyEl.scrollHeight;
        var textEl = div.querySelector('.v360cb__text');
        var accumulated = '';
        return {
            append: function (delta) {
                accumulated += delta;
                textEl.innerHTML = escHtml(accumulated).replace(/\n/g, '<br>');
                bodyEl.scrollTop = bodyEl.scrollHeight;
            },
            setText: function (text) {
                accumulated = text;
                textEl.innerHTML = escHtml(text).replace(/\n/g, '<br>');
                bodyEl.scrollTop = bodyEl.scrollHeight;
            },
            getText: function () { return accumulated; },
            finalize: function (navTarget, navName) {
                div.classList.remove('v360cb__msg--streaming');
                var cursor = div.querySelector('.v360cb__cursor');
                if (cursor) cursor.remove();
                if (navTarget) {
                    var chip = document.createElement('div');
                    chip.className = 'v360cb__nav';
                    chip.innerHTML = '<i class="fa fa-location-arrow"></i> Đang đưa bạn tới: ' +
                                     escHtml(navName || navTarget);
                    div.appendChild(chip);
                }
                bodyEl.scrollTop = bodyEl.scrollHeight;
                return accumulated;
            }
        };
    }

    // Parse 1 batch SSE events tu chunk text (co the chua nhieu events hoac event do dang).
    // Tra ve { events: [{type, data}], remainder: string }.
    function parseSseChunk(buffer) {
        var events = [];
        var parts = buffer.split(/\n\n/);
        var remainder = parts.pop(); // truc cuoi chua hoan thanh
        parts.forEach(function (block) {
            var lines = block.split('\n');
            var evtType = 'message', dataStr = '';
            lines.forEach(function (ln) {
                if (ln.indexOf('event:') === 0) evtType = ln.substring(6).trim();
                else if (ln.indexOf('data:') === 0) dataStr += ln.substring(5).trimStart();
            });
            if (dataStr) {
                try { events.push({ type: evtType, data: JSON.parse(dataStr) }); }
                catch (e) { console.warn('[v360cb] SSE parse err', e, dataStr); }
            }
        });
        return { events: events, remainder: remainder };
    }

    function send() {
        if (busy) return;
        var text = inputEl.value.trim();
        if (!text) return;
        if (text.length > 500) text = text.substring(0, 500);

        clearSuggestions();
        if (typeof stopTtsAudio === 'function') stopTtsAudio();
        appendMsg('user', text);
        history.push({ role: 'user', content: text });
        inputEl.value = '';
        setBusy(true);
        appendTyping();
        var responseMode = effectiveMode();

        var sc = currentScene();
        var payload = {
            collectionId: COLLECTION_ID,
            sceneUuid:    sc.uuid || null,
            sessionGuid:  SESSION_GUID,
            message:      text,
            history:      history.slice(-10)
        };

        var bubble = null;
        var navTarget = null, navName = null;
        var sawError = false;
        var doneSuggestions = null;
        // TTS mode: accumulate text trong invisible buffer, KHONG show streaming.
        // Sau khi stream done + TTS audio loaded -> reveal text sync voi audio.
        // responseMode phai co dinh theo luc bam gui; doi tab mode giua chung chi ap dung cho cau sau.
        var ttsBuffer = '';

        fetch(ASK_URL, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'Accept': 'text/event-stream' },
            body: JSON.stringify(payload)
        }).then(function (res) {
            if (!res.ok) throw new Error('HTTP ' + res.status);
            if (!res.body || !res.body.getReader) {
                throw new Error('Trình duyệt không hỗ trợ streaming');
            }
            var reader = res.body.getReader();
            var decoder = new TextDecoder('utf-8');
            var buffer = '';
            function pump() {
                return reader.read().then(function (r) {
                    if (r.done) return;
                    buffer += decoder.decode(r.value, { stream: true });
                    var parsed = parseSseChunk(buffer);
                    buffer = parsed.remainder;
                    parsed.events.forEach(function (e) {
                        if (e.type === 'text') {
                            if (responseMode === 'tts') {
                                // Accumulate silent; bubble se duoc tao sau khi audio start.
                                // Giu typing indicator de UI mượt.
                                ttsBuffer += (e.data.delta || '');
                            } else {
                                if (!bubble) {
                                    removeTyping();
                                    bubble = createStreamingBubble();
                                }
                                bubble.append(e.data.delta || '');
                            }
                        } else if (e.type === 'done') {
                            if (e.data.action && e.data.action.type === 'navigate') {
                                navTarget = e.data.action.target;
                                navName = e.data.action.name || null;
                                if (!navName) {
                                    try {
                                        if (window.SCENE_CUSTOM_TITLE && window.SCENE_CUSTOM_TITLE[navTarget]) {
                                            navName = window.SCENE_CUSTOM_TITLE[navTarget];
                                        }
                                    } catch (_) {}
                                }
                            }
                            if (e.data.suggestions && e.data.suggestions.length) {
                                doneSuggestions = e.data.suggestions;
                            }
                        } else if (e.type === 'error') {
                            sawError = true;
                            removeTyping();
                            appendMsg('error', e.data.error || 'Lỗi không xác định');
                        }
                    });
                    return pump();
                });
            }
            return pump();
        }).then(function () {
            setBusy(false);
            if (sawError) { removeTyping(); return; }

            // TTS mode (hoac Voice PTT -> reply doc to): text da accumulate trong ttsBuffer
            if (responseMode === 'tts') {
                var ttsCleanText = ttsBuffer.replace(/\s*---SUGGEST---[\s\S]*$/, '').trim();
                if (!ttsCleanText) {
                    removeTyping();
                    appendMsg('bot', 'Xin lỗi, tôi chưa có câu trả lời cho câu hỏi này.');
                    history.push({ role: 'assistant', content: '' });
                    return;
                }
                history.push({ role: 'assistant', content: ttsCleanText });
                // Sync reveal: keep typing -> when audio loaded, hide typing + create bubble + reveal sync
                speakAndRevealSynced(ttsCleanText, navTarget, navName, doneSuggestions);
                return;
            }

            // Text mode: bubble da hien streaming -> finalize
            removeTyping();
            if (!bubble) {
                appendMsg('bot', 'Xin lỗi, tôi chưa có câu trả lời cho câu hỏi này.');
                history.push({ role: 'assistant', content: '' });
                return;
            }
            var finalText = bubble.finalize(navTarget, navName);
            var cleanText = finalText.replace(/\s*---SUGGEST---[\s\S]*$/, '').trim();
            history.push({ role: 'assistant', content: cleanText });
            if (doneSuggestions) renderSuggestions(doneSuggestions);
            if (navTarget) {
                setTimeout(function () { tryNavigate(navTarget, navName); }, 700);
            }
        }).catch(function (e) {
            removeTyping();
            setBusy(false);
            if (!sawError) appendMsg('error', 'Lỗi mạng: ' + e.message);
        });
    }

    sendBtn.addEventListener('click', send);
    inputEl.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            send();
        }
    });

    // ================================================================
    //  MODE: text | tts (text -> voice) | voice (push-to-talk via VBee STT)
    // ================================================================
    var MODE = 'text';
    // Voice mode (PTT) -> reply duoc doc to bang VBee TTS, behave nhu TTS mode trong send/SSE flow.
    function effectiveMode() { return MODE === 'voice' ? 'tts' : MODE; }
    var VOICE_URL = window.V360CB_VOICE_URL || '';
    var TTS_URL   = window.V360CB_TTS_URL || '';
    var STT_URL   = window.V360CB_STT_URL || '';

    // Push-to-talk state (voice mode via VBee STT batch)
    var pttState = 'idle';  // idle | ready | recording | processing
    var pttStream = null;
    var pttAudioCtx = null;
    var pttSourceNode = null;
    var pttProcessorNode = null;
    var pttSamples = [];   // Float32Array chunks captured tu ScriptProcessor
    // pttRecorder giu lai de pttCleanup khong ref undefined (legacy MediaRecorder)
    var pttRecorder = null;
    var pttChunks = [];

    // PTT button (insert before send button in input wrap)
    var pttBtn = document.createElement('button');
    pttBtn.type = 'button';
    pttBtn.className = 'v360cb__ptt';
    pttBtn.title = 'Bấm để nói';
    pttBtn.innerHTML = '<i class="fa fa-microphone"></i>';
    pttBtn.style.display = 'none';
    panel.querySelector('.v360cb__input-wrap').insertBefore(pttBtn, sendBtn);
    var voiceState = 'idle'; // idle | connecting | active (chi voi mode = voice)
    var voicePc = null, voiceDc = null, voiceMic = null, voiceAudioEl = null;
    var voiceBubble = null;
    var voiceTimeout = null;
    var VOICE_MAX_MS = 180000;
    var voiceStatus = document.getElementById('v360cb-vstat');

    // Mode selector buttons
    var modeBtns = panel.querySelectorAll('.v360cb__mode');
    modeBtns.forEach(function (b) {
        b.addEventListener('click', function () { setMode(b.dataset.mode); });
    });

    function setMode(newMode) {
        if (newMode === MODE) return;
        if (MODE === 'voice' && voiceState !== 'idle') stopVoice();    // OpenAI realtime cleanup (legacy)
        if (MODE === 'voice') pttCleanup();                              // VBee PTT cleanup
        MODE = newMode;
        modeBtns.forEach(function (b) {
            b.classList.toggle('is-active', b.dataset.mode === MODE);
        });
        if (MODE === 'voice') {
            inputEl.placeholder = '// Bấm mic để nói';
            inputEl.disabled = true;
            sendBtn.style.display = 'none';
            pttBtn.style.display = 'flex';
            pttSetState('ready');
        } else if (MODE === 'tts') {
            inputEl.placeholder = '// Nhập câu hỏi (AI sẽ đọc câu trả lời)';
            inputEl.disabled = false;
            sendBtn.style.display = 'flex';
            pttBtn.style.display = 'none';
            voiceStatus.style.display = 'none';
        } else { // text
            inputEl.placeholder = '// Nhập câu hỏi...';
            inputEl.disabled = false;
            sendBtn.style.display = 'flex';
            pttBtn.style.display = 'none';
            voiceStatus.style.display = 'none';
        }
    }

    // ================================================================
    //  PUSH-TO-TALK (VBee STT batch). Click pttBtn de start, click lai de stop.
    //  Auto-stop sau 10s (VBee sync limit).
    // ================================================================
    function pttSetState(state) {
        pttState = state;
        pttBtn.classList.remove('is-recording', 'is-processing');
        if (state === 'ready') {
            pttBtn.innerHTML = '<i class="fa fa-microphone"></i>';
            pttBtn.title = 'Bấm để nói';
            voiceStatus.innerHTML = '<i class="fa fa-info-circle"></i> Bấm mic để bắt đầu ghi âm (tối đa 10 giây)';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status';
        } else if (state === 'recording') {
            pttBtn.classList.add('is-recording');
            pttBtn.innerHTML = '<i class="fa fa-stop"></i>';
            pttBtn.title = 'Bấm để dừng';
            voiceStatus.innerHTML = '<span class="v360cb__voice-pulse is-user"></span> Đang ghi âm... (bấm lại để dừng)';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status is-active';
        } else if (state === 'processing') {
            pttBtn.classList.add('is-processing');
            pttBtn.innerHTML = '<i class="fa fa-circle-notch fa-spin"></i>';
            pttBtn.title = 'Đang xử lý';
            voiceStatus.innerHTML = '<i class="fa fa-cog fa-spin"></i> Đang chuyển giọng nói thành văn bản...';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status is-connecting';
        }
    }

    function pttCleanup() {
        try { if (pttProcessorNode) { pttProcessorNode.disconnect(); pttProcessorNode.onaudioprocess = null; } } catch (_) {}
        try { if (pttSourceNode) pttSourceNode.disconnect(); } catch (_) {}
        if (pttStream) { try { pttStream.getTracks().forEach(function (t) { t.stop(); }); } catch (_) {} }
        if (pttAudioCtx) { try { pttAudioCtx.close(); } catch (_) {} }
        pttProcessorNode = null;
        pttSourceNode = null;
        pttAudioCtx = null;
        pttStream = null;
        pttSamples = [];
        pttState = 'idle';
        pttBtn.classList.remove('is-recording', 'is-processing');
    }

    // Encode Float32 PCM samples -> WAV (16-bit mono). VBee STT chi accept WAV.
    function encodeWav(samples, sampleRate) {
        var len = samples.length;
        var buffer = new ArrayBuffer(44 + len * 2);
        var view = new DataView(buffer);
        function ws(off, s) { for (var i = 0; i < s.length; i++) view.setUint8(off + i, s.charCodeAt(i)); }
        ws(0, 'RIFF');
        view.setUint32(4, 36 + len * 2, true);
        ws(8, 'WAVE');
        ws(12, 'fmt ');
        view.setUint32(16, 16, true);                  // fmt chunk size
        view.setUint16(20, 1, true);                   // PCM
        view.setUint16(22, 1, true);                   // mono
        view.setUint32(24, sampleRate, true);
        view.setUint32(28, sampleRate * 2, true);      // byte rate (16-bit mono)
        view.setUint16(32, 2, true);                   // block align
        view.setUint16(34, 16, true);                  // bits per sample
        ws(36, 'data');
        view.setUint32(40, len * 2, true);
        var off = 44;
        for (var i = 0; i < len; i++) {
            var s = Math.max(-1, Math.min(1, samples[i]));
            view.setInt16(off, s < 0 ? s * 0x8000 : s * 0x7FFF, true);
            off += 2;
        }
        return new Blob([buffer], { type: 'audio/wav' });
    }

    function pttDbfs(rms) {
        if (!rms || rms <= 0) return '-inf';
        return (20 * Math.log10(rms)).toFixed(1);
    }

    function pttNormalizeForStt(samples) {
        var len = samples.length;
        if (!len) {
            return {
                samples: samples,
                stats: { inputRms: 0, inputPeak: 0, outputRms: 0, outputPeak: 0, gain: 1, clippedPercent: 0 }
            };
        }

        var sum = 0;
        for (var i = 0; i < len; i++) sum += samples[i];
        var mean = sum / len;

        var sumSq = 0, peak = 0;
        for (var j = 0; j < len; j++) {
            var centered = samples[j] - mean;
            var abs = Math.abs(centered);
            if (abs > peak) peak = abs;
            sumSq += centered * centered;
        }
        var rms = Math.sqrt(sumSq / len);

        var targetRms = 0.14;  // approx -17 dBFS, good for speech STT
        var maxGain = 14;
        var gain = rms > 0.0001 ? (targetRms / rms) : 1;
        if (gain < 1) gain = 1;
        if (gain > maxGain) gain = maxGain;

        var out = new Float32Array(len);
        var outSumSq = 0, outPeak = 0, clipped = 0;
        for (var k = 0; k < len; k++) {
            var v = (samples[k] - mean) * gain;
            if (v > 0.98) { v = 0.98; clipped++; }
            else if (v < -0.98) { v = -0.98; clipped++; }
            out[k] = v;
            var outAbs = Math.abs(v);
            if (outAbs > outPeak) outPeak = outAbs;
            outSumSq += v * v;
        }

        return {
            samples: out,
            stats: {
                inputRms: rms,
                inputPeak: peak,
                outputRms: Math.sqrt(outSumSq / len),
                outputPeak: outPeak,
                gain: gain,
                clippedPercent: clipped * 100 / len
            }
        };
    }

    async function pttStart() {
        if (pttState !== 'ready') return;
        if (typeof stopTtsAudio === 'function') stopTtsAudio();
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            appendMsg('error', 'Trình duyệt không hỗ trợ microphone (cần HTTPS hoặc localhost).');
            return;
        }
        var Ctx = window.AudioContext || window.webkitAudioContext;
        if (!Ctx) {
            appendMsg('error', 'Trình duyệt không hỗ trợ Web Audio API.');
            return;
        }
        try {
            pttStream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    channelCount: { ideal: 1 },
                    sampleRate: { ideal: 16000 },
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                }
            });
        } catch (mErr) {
            var msg;
            switch (mErr.name) {
                case 'NotFoundError':
                case 'OverconstrainedError': msg = 'Máy không có microphone.'; break;
                case 'NotAllowedError':
                case 'SecurityError': msg = 'Bạn chưa cấp quyền microphone.'; break;
                case 'NotReadableError': msg = 'Microphone đang bị app khác sử dụng.'; break;
                default: msg = 'Không truy cập được microphone: ' + (mErr.message || mErr.name);
            }
            appendMsg('error', msg);
            return;
        }
        // Yeu cau 16kHz cho VBee STT; mot so browser khong honor -> dung sampleRate thuc te khi encode WAV
        try { pttAudioCtx = new Ctx({ sampleRate: 16000 }); }
        catch (_) { try { pttAudioCtx = new Ctx(); } catch (e2) {
            appendMsg('error', 'Không khởi tạo AudioContext: ' + e2.message);
            pttCleanup(); return;
        }}
        pttSamples = [];
        try {
            pttSourceNode = pttAudioCtx.createMediaStreamSource(pttStream);
            pttProcessorNode = pttAudioCtx.createScriptProcessor(4096, 1, 1);
        } catch (e) {
            appendMsg('error', 'Không khởi tạo Audio nodes: ' + e.message);
            pttCleanup(); return;
        }
        pttProcessorNode.onaudioprocess = function (e) {
            var input = e.inputBuffer.getChannelData(0);
            // Phai copy vi inputBuffer.getChannelData tra Float32Array dung chung buffer
            pttSamples.push(new Float32Array(input));
        };
        pttSourceNode.connect(pttProcessorNode);
        // Phai connect destination de onaudioprocess fire tren mot so browser (Chrome quirks)
        pttProcessorNode.connect(pttAudioCtx.destination);
        pttSetState('recording');
        setTimeout(function () { if (pttState === 'recording') pttStop(); }, 10000);
    }

    function pttStop() {
        if (pttState !== 'recording') return;
        pttSetState('processing');
        var sampleRate = pttAudioCtx ? pttAudioCtx.sampleRate : 16000;
        try { if (pttProcessorNode) { pttProcessorNode.disconnect(); pttProcessorNode.onaudioprocess = null; } } catch (_) {}
        try { if (pttSourceNode) pttSourceNode.disconnect(); } catch (_) {}
        if (pttStream) { try { pttStream.getTracks().forEach(function (t) { t.stop(); }); } catch (_) {} }
        if (pttAudioCtx) { try { pttAudioCtx.close(); } catch (_) {} }
        pttProcessorNode = null;
        pttSourceNode = null;
        pttAudioCtx = null;

        if (!pttSamples.length) {
            appendMsg('error', 'Không ghi âm được gì.');
            pttSetState('ready');
            return;
        }
        // Flatten Float32 chunks
        var total = 0;
        for (var i = 0; i < pttSamples.length; i++) total += pttSamples[i].length;
        var merged = new Float32Array(total);
        var off = 0;
        for (var k = 0; k < pttSamples.length; k++) {
            merged.set(pttSamples[k], off);
            off += pttSamples[k].length;
        }
        pttSamples = [];
        var durationSeconds = merged.length / sampleRate;
        var normalized = pttNormalizeForStt(merged);
        console.log('[v360cb-ptt] WAV encode: ' + merged.length + ' samples @ ' + sampleRate + 'Hz = '
            + durationSeconds.toFixed(2) + 's, gain x' + normalized.stats.gain.toFixed(2)
            + ', rms ' + pttDbfs(normalized.stats.inputRms) + ' -> ' + pttDbfs(normalized.stats.outputRms) + ' dBFS');
        var wavBlob = encodeWav(normalized.samples, sampleRate);
        pttUpload(wavBlob, sampleRate, durationSeconds, normalized.stats);
    }

    function pttUpload(blob, sampleRate, durationSeconds, gainStats) {
        if (!STT_URL) {
            appendMsg('error', 'STT URL chưa cấu hình.');
            pttSetState('ready');
            return;
        }
        var fd = new FormData();
        fd.append('audio', blob, 'recording.wav');
        fd.append('clientSampleRate', String(sampleRate || ''));
        fd.append('clientDurationSeconds', durationSeconds ? durationSeconds.toFixed(3) : '');
        if (gainStats) {
            fd.append('clientInputRms', String(gainStats.inputRms || 0));
            fd.append('clientOutputRms', String(gainStats.outputRms || 0));
            fd.append('clientGain', String(gainStats.gain || 1));
            fd.append('clientClippedPercent', String(gainStats.clippedPercent || 0));
        }

        fetch(STT_URL, { method: 'POST', credentials: 'same-origin', body: fd })
            .then(function (r) { return r.json(); })
            .then(function (j) {
                if (!j.ok) throw new Error(j.error || 'STT fail');
                var transcript = (j.transcript || '').trim();
                if (!transcript) throw new Error('Không nhận diện được giọng nói');
                console.log('[v360cb-ptt] transcript:', transcript);
                voiceStatus.innerHTML = '<i class="fa fa-check"></i> "' + escHtml(transcript) + '"';
                voiceStatus.className = 'v360cb__voice-status is-connecting';
                // KHONG hack MODE - dung effectiveMode() trong send/SSE flow de voice mode
                // tu dong di duong TTS (reply doc to bang VBee). Tham khao effectiveMode().
                inputEl.value = transcript;
                inputEl.disabled = false;
                send();
                inputEl.disabled = true;
                setTimeout(function () { if (MODE === 'voice') pttSetState('ready'); }, 800);
            })
            .catch(function (e) {
                console.warn('[v360cb-ptt]', e);
                appendMsg('error', '🎙 Lỗi STT: ' + e.message);
                if (MODE === 'voice') pttSetState('ready');
            })
            .finally(function () {
                pttStream = null;
            });
    }

    pttBtn.addEventListener('click', function () {
        if (pttState === 'ready') pttStart();
        else if (pttState === 'recording') pttStop();
    });

    function setVoiceUI(state, msg) {
        voiceState = state;
        if (state === 'idle') {
            voiceStatus.style.display = 'none';
            voiceStatus.className = 'v360cb__voice-status';
        } else if (state === 'connecting') {
            voiceStatus.innerHTML = '<i class="fa fa-circle-notch fa-spin"></i> ' + (msg || 'Đang kết nối...');
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status is-connecting';
        } else if (state === 'active') {
            voiceStatus.innerHTML = '<span class="v360cb__voice-pulse"></span> Đang nghe... (đổi mode để dừng)';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status is-active';
        }
    }

    function normalizeText(s) {
        if (!s) return '';
        return String(s).normalize('NFD').replace(/[̀-ͯ]/g, '')
                        .toLowerCase().replace(/đ/g, 'd');
    }

    // Client-side fuzzy match scene query (su dung SCENE_CUSTOM_TITLE + Kuula post titles).
    // Reuse logic server-side FindSceneByQuery (CustomTitle > Kuula title).
    function findSceneClient(query) {
        if (!query) return null;
        var q = normalizeText(query.trim());
        if (q.length < 2) return null;
        var titles = window.SCENE_CUSTOM_TITLE || {};
        var allPosts = (typeof window.__v360GetAllPosts === 'function') ? window.__v360GetAllPosts() : [];
        var current = (typeof window.__v360CurrentScene === 'function') ? window.__v360CurrentScene() : { uuid: null };

        var best = null, bestScore = 0;
        // 1. CustomTitle
        for (var uuid in titles) {
            if (uuid === current.uuid) continue;
            var n = normalizeText(titles[uuid] || '');
            var score = 0;
            if (n === q) score = 200;
            else if (n.indexOf(q) === 0) score = 150;
            else if (n.indexOf(q) >= 0) score = 120;
            if (score > bestScore) { bestScore = score; best = { uuid: uuid, name: titles[uuid] }; }
        }
        // 2. Kuula post titles (fallback)
        if (bestScore < 100 && allPosts && allPosts.length) {
            for (var i = 0; i < allPosts.length; i++) {
                var p = allPosts[i];
                var pkey = p._uuid || p.uuid;
                if (!pkey || pkey === current.uuid) continue;
                if (!p.title) continue;
                var pn = normalizeText(p.title);
                if (pn.indexOf(q) >= 0 && bestScore < 30) {
                    bestScore = 30;
                    best = { uuid: pkey, name: titles[pkey] || p.title };
                }
            }
        }
        return best;
    }

    async function startVoice() {
        if (voiceState !== 'idle') return;
        if (!VOICE_URL) {
            appendMsg('error', 'Voice URL chưa cấu hình');
            return;
        }
        setVoiceUI('connecting', 'Đang lấy mic + kết nối...');

        try {
            // 1. CHECK MIC TRUOC (fail-fast - tranh waste session create neu khong co mic)
            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                throw new Error('Trình duyệt không hỗ trợ microphone (cần HTTPS hoặc localhost).');
            }
            try {
                voiceMic = await navigator.mediaDevices.getUserMedia({ audio: true });
            } catch (mErr) {
                var msg;
                switch (mErr.name) {
                    case 'NotFoundError':
                    case 'OverconstrainedError':
                        msg = 'Máy không có microphone. Hãy cắm tai nghe có mic, hoặc chuyển sang chế độ Text/Đọc.';
                        break;
                    case 'NotAllowedError':
                    case 'SecurityError':
                        msg = 'Bạn chưa cấp quyền microphone. Bấm vào icon ổ khóa trên thanh địa chỉ để cấp quyền, hoặc dùng chế độ Text.';
                        break;
                    case 'NotReadableError':
                        msg = 'Microphone đang bị app khác sử dụng (Zoom/Teams/Discord?). Hãy đóng app đó rồi thử lại.';
                        break;
                    default:
                        msg = 'Không truy cập được microphone: ' + (mErr.message || mErr.name);
                }
                throw new Error(msg);
            }

            // 2. Ephemeral session tu server (sau khi xac nhan co mic)
            var sc = currentScene();
            var sessRes = await fetch(VOICE_URL, {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    collectionId: window.V360CB_COLLECTION_ID,
                    sceneUuid: sc.uuid || null
                })
            });
            if (!sessRes.ok) throw new Error('HTTP ' + sessRes.status);
            var sess = await sessRes.json();
            if (!sess.ok) throw new Error(sess.error || 'session fail');

            // 3. PeerConnection
            voicePc = new RTCPeerConnection();

            // 4. Audio playback element (an, autoplay)
            voiceAudioEl = document.createElement('audio');
            voiceAudioEl.autoplay = true;
            voicePc.ontrack = function (e) { voiceAudioEl.srcObject = e.streams[0]; };

            voiceMic.getAudioTracks().forEach(function (t) { voicePc.addTrack(t, voiceMic); });

            // 5. Data channel
            voiceDc = voicePc.createDataChannel('oai-events');
            voiceDc.addEventListener('open', function () { console.log('[v360cb-voice] dc open'); });
            voiceDc.addEventListener('message', handleVoiceEvent);

            // 6. SDP exchange
            var offer = await voicePc.createOffer();
            await voicePc.setLocalDescription(offer);

            var sdpResp = await fetch(
                'https://api.openai.com/v1/realtime?model=' + encodeURIComponent(sess.model),
                {
                    method: 'POST',
                    body: offer.sdp,
                    headers: {
                        'Authorization': 'Bearer ' + sess.clientSecret,
                        'Content-Type': 'application/sdp'
                    }
                });
            if (!sdpResp.ok) {
                var errText = await sdpResp.text();
                throw new Error('SDP HTTP ' + sdpResp.status + ': ' + errText.substring(0, 200));
            }
            var answerSdp = await sdpResp.text();
            await voicePc.setRemoteDescription({ type: 'answer', sdp: answerSdp });

            setVoiceUI('active');
            setKuulaMuted(true);  // mute Kuula audio trong suot voice session
            console.log('[v360cb-voice] connected');

            // Auto stop sau VOICE_MAX_MS
            voiceTimeout = setTimeout(function () {
                appendMsg('bot', 'Đã dừng voice mode (hết thời gian phiên).');
                stopVoice();
            }, VOICE_MAX_MS);
        } catch (e) {
            console.error('[v360cb-voice] start err', e);
            appendMsg('error', e.message || 'Không bật được voice');
            stopVoice();
            // Auto fallback ve Text mode (de UI khong bi ket o voice)
            var textBtn = panel.querySelector('.v360cb__mode[data-mode="text"]');
            if (textBtn && MODE === 'voice') {
                setTimeout(function () { setMode('text'); }, 200);
            }
        }
    }

    function stopVoice() {
        if (voiceTimeout) { clearTimeout(voiceTimeout); voiceTimeout = null; }
        if (voiceDc) { try { voiceDc.close(); } catch(_) {} voiceDc = null; }
        if (voicePc) { try { voicePc.close(); } catch(_) {} voicePc = null; }
        if (voiceMic) { voiceMic.getTracks().forEach(function (t) { t.stop(); }); voiceMic = null; }
        if (voiceAudioEl) { try { voiceAudioEl.pause(); voiceAudioEl.srcObject = null; } catch(_) {} voiceAudioEl = null; }
        voiceBubble = null;
        setVoiceUI('idle');
        setKuulaMuted(false);  // restore Kuula audio
    }

    function handleVoiceEvent(e) {
        var msg;
        try { msg = JSON.parse(e.data); } catch (_) { return; }
        // console.log('[v360cb-voice]', msg.type, msg);
        switch (msg.type) {
            case 'session.created':
            case 'session.updated':
                break;
            case 'conversation.item.input_audio_transcription.completed':
                // User noi xong, transcript ve
                if (msg.transcript) {
                    appendMsg('user', '🎙 ' + msg.transcript);
                    history.push({ role: 'user', content: msg.transcript });
                }
                break;
            case 'response.audio_transcript.delta':
                // AI noi - transcript streaming
                if (!voiceBubble) {
                    removeTyping();
                    voiceBubble = createStreamingBubble();
                }
                if (msg.delta) voiceBubble.append(msg.delta);
                break;
            case 'response.audio_transcript.done':
                if (voiceBubble) {
                    var finalText = voiceBubble.finalize(null, null);
                    history.push({ role: 'assistant', content: finalText });
                    voiceBubble = null;
                }
                break;
            case 'response.function_call_arguments.done':
                handleVoiceToolCall(msg);
                break;
            case 'input_audio_buffer.speech_started':
                voiceStatus.innerHTML = '<span class="v360cb__voice-pulse is-user"></span> Bạn đang nói...';
                break;
            case 'input_audio_buffer.speech_stopped':
                voiceStatus.innerHTML = '<span class="v360cb__voice-pulse"></span> Đang xử lý...';
                break;
            case 'output_audio_buffer.started':
                voiceStatus.innerHTML = '<span class="v360cb__voice-pulse is-bot"></span> AI đang trả lời...';
                break;
            case 'output_audio_buffer.stopped':
            case 'response.done':
                if (voiceState === 'active')
                    voiceStatus.innerHTML = '<span class="v360cb__voice-pulse"></span> Đang nghe... (bấm mic để dừng)';
                break;
            case 'error':
                console.error('[v360cb-voice] error event', msg);
                appendMsg('error', 'Voice error: ' + (msg.error && msg.error.message || ''));
                break;
        }
    }

    function handleVoiceToolCall(msg) {
        if (msg.name !== 'navigate_to_scene') return;
        var args;
        try { args = JSON.parse(msg.arguments); } catch (_) { args = {}; }
        var query = args.query || '';
        var match = findSceneClient(query);
        var output;
        if (match) {
            console.log('[v360cb-voice] navigate', query, '->', match);
            tryNavigate(match.uuid, match.name);
            output = { ok: true, name: match.name };
        } else {
            console.warn('[v360cb-voice] navigate not found', query);
            output = { ok: false, error: 'Không tìm thấy điểm "' + query + '"' };
        }
        // Send function output back + trigger AI tiep tuc
        if (voiceDc && voiceDc.readyState === 'open') {
            voiceDc.send(JSON.stringify({
                type: 'conversation.item.create',
                item: {
                    type: 'function_call_output',
                    call_id: msg.call_id,
                    output: JSON.stringify(output)
                }
            }));
            voiceDc.send(JSON.stringify({ type: 'response.create' }));
        }
    }

    // Khi dong panel: dung voice neu dang active
    closeBtn.addEventListener('click', function () { if (voiceState !== 'idle') stopVoice(); });

    // ================================================================
    //  TTS PLAYBACK - cho mode "tts" (text -> voice).
    //  speakAndRevealSynced: fetch TTS audio, reveal text incrementally
    //  match audio.currentTime - UI mượt thay vi text nhay ra truoc audio.
    // ================================================================
    var ttsAudioEl = null;
    var ttsRevealTicker = null;

    // Mute/unmute Kuula iframe audio - thu nhieu Kuula API variants vi cross-origin
    // KHONG biet chac Kuula expose command nao. Tat ca silent fail neu Kuula khong support.
    function setKuulaMuted(muted) {
        if (typeof window.kuulaSend !== 'function') return;
        try {
            window.kuulaSend(muted ? 'mute' : 'unmute');
            window.kuulaSend('audio', { value: muted ? 0 : 1 });
            window.kuulaSend('volume', { value: muted ? 0 : 100 });
        } catch (_) {}
    }

    function cleanTextForTts(text) {
        return String(text || '')
            .replace(/[*_`~]/g, '')      // markdown emphasis
            .replace(/\[.*?\]/g, '')     // brackets
            .replace(/\([\s\S]*?\)/g, function(m){ return m.length < 60 ? m : ''; })  // long parens (likely not speech)
            .trim();
    }

    function stopTtsAudio() {
        if (ttsRevealTicker) { clearInterval(ttsRevealTicker); ttsRevealTicker = null; }
        if (ttsAudioEl) { try { ttsAudioEl.pause(); ttsAudioEl.srcObject = null; } catch(_) {} ttsAudioEl = null; }
        setKuulaMuted(false);  // restore Kuula audio
    }

    function speakAndRevealSynced(fullText, navTarget, navName, suggestions) {
        if (!TTS_URL || !fullText) {
            // Fallback: hien luon text + finalize, khong audio
            removeTyping();
            var b = createStreamingBubble();
            b.setText(fullText);
            b.finalize(navTarget, navName);
            if (suggestions) renderSuggestions(suggestions);
            if (navTarget) setTimeout(function(){ tryNavigate(navTarget, navName); }, 700);
            return;
        }
        stopTtsAudio();
        var speechText = cleanTextForTts(fullText);

        fetch(TTS_URL, {
            method: 'POST', credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text: speechText })
        })
        .then(function (r) {
            if (!r.ok) {
                // Doc body de bao loi cu the (server tra JSON khi loi)
                return r.text().then(function (bodyText) {
                    var detail = bodyText;
                    try {
                        var j = JSON.parse(bodyText);
                        detail = j.detail || j.error || bodyText;
                    } catch (_) {}
                    console.error('[v360cb-tts] HTTP', r.status, detail);
                    throw new Error('TTS lỗi (' + r.status + '): ' + String(detail).substring(0, 200));
                });
            }
            return r.blob();
        })
        .then(function (blob) {
            var blobUrl = URL.createObjectURL(blob);
            ttsAudioEl = new Audio(blobUrl);
            var bubble = null;
            var revealed = 0;
            var finalized = false;  // chong double finalize -> double nav chip

            function startReveal() {
                if (bubble) return; // already started
                removeTyping();
                bubble = createStreamingBubble();
                // Tick 50ms cap nhat text theo audio.currentTime
                ttsRevealTicker = setInterval(function () {
                    if (!ttsAudioEl) { clearInterval(ttsRevealTicker); ttsRevealTicker = null; return; }
                    var dur = ttsAudioEl.duration;
                    if (!isFinite(dur) || dur < 0.3) return;
                    var progress = Math.min(1, ttsAudioEl.currentTime / dur);
                    var targetCount = Math.ceil(fullText.length * progress);
                    if (targetCount > revealed) {
                        revealed = targetCount;
                        bubble.setText(fullText.substring(0, revealed));
                    }
                    if (progress >= 1) {
                        clearInterval(ttsRevealTicker); ttsRevealTicker = null;
                    }
                }, 60);
            }
            function finishReveal() {
                if (finalized) return;   // idempotent - chong duplicate nav chip
                finalized = true;
                if (ttsRevealTicker) { clearInterval(ttsRevealTicker); ttsRevealTicker = null; }
                if (!bubble) {
                    removeTyping();
                    bubble = createStreamingBubble();
                }
                bubble.setText(fullText);
                bubble.finalize(navTarget, navName);
                if (suggestions) renderSuggestions(suggestions);
                if (navTarget) setTimeout(function () { tryNavigate(navTarget, navName); }, 400);
                try { URL.revokeObjectURL(blobUrl); } catch (_) {}
            }

            ttsAudioEl.addEventListener('playing', function () {
                setKuulaMuted(true);   // mute Kuula khi TTS bat dau noi
                startReveal();
            });
            ttsAudioEl.addEventListener('ended', finishReveal);
            ttsAudioEl.addEventListener('error', function (e) {
                console.warn('[v360cb-tts] audio err', e);
                finishReveal();
            });
            // Safety: neu audio khong play duoc (no autoplay permission, error...) -> show text sau 4s
            setTimeout(function () { if (!bubble) finishReveal(); }, 4000);

            ttsAudioEl.play().catch(function (e) {
                console.warn('[v360cb-tts] play() rejected', e);
                // Browser autoplay policy block - hien text + bao user
                finishReveal();
                appendMsg('error', 'Trình duyệt chặn auto-play audio. Bấm vào tab này 1 lần rồi thử lại.');
            });
        })
        .catch(function (e) {
            console.warn('[v360cb-tts]', e);
            // Fallback: hien text + bao loi cho user
            removeTyping();
            var b = createStreamingBubble();
            b.setText(fullText);
            b.finalize(navTarget, navName);
            if (suggestions) renderSuggestions(suggestions);
            if (navTarget) setTimeout(function(){ tryNavigate(navTarget, navName); }, 700);
            appendMsg('error', '🔊 ' + (e.message || 'TTS không khả dụng'));
        });
    }

    console.log('[v360cb] widget mounted, session=' + SESSION_GUID);
})();
