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
        // Dong panel -> tat mic ngay (rieng tu + tranh thu am khi khong dung).
        if (MODE === 'voice' && typeof pttCleanup === 'function') pttCleanup();
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
            html += '<div class="v360cb__nav v360cb__nav--pending"><i class="fa fa-location-arrow"></i> ' +
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
                        watchArrival(uuid, displayName);
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

    // Sau khi kuulaSend('load'), theo doi __v360CurrentScene cho den khi scene dich load xong
    // -> cap nhat chip "Đang đưa bạn tới" thanh "Đã tới" (xac nhan da chuyen canh thanh cong).
    function markNavArrived(name) {
        var pend = bodyEl.querySelectorAll('.v360cb__nav--pending');
        var chip = pend[pend.length - 1];   // chip pending moi nhat = nav vua roi
        if (!chip) return;
        chip.classList.remove('v360cb__nav--pending');
        chip.classList.add('v360cb__nav--done');
        var label = (name && String(name).trim()) ? escHtml(name) : 'điểm đến';
        chip.innerHTML = '<i class="fa fa-check-circle"></i> Đã tới: ' + label;
        bodyEl.scrollTop = bodyEl.scrollHeight;
    }
    function watchArrival(uuid, displayName) {
        var target = String(uuid || '').toLowerCase().replace(/-/g, '');
        if (!target) return;
        var tries = 0;
        (function check() {
            tries++;
            var cur = currentScene();
            var curU = String((cur && cur.uuid) || '').toLowerCase().replace(/-/g, '');
            if (curU && curU === target) {
                markNavArrived(displayName || (cur && cur.name) || '');
                return;
            }
            if (tries < 40) setTimeout(check, 300);  // poll ~12s; het thi giu nguyen "Đang đưa..."
        })();
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
                    chip.className = 'v360cb__nav v360cb__nav--pending';
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
        // TTS pipeline: tach cau + phat tuan tu NGAY trong luc LLM dang stream (giam do tre).
        var ttsPipe = (responseMode === 'tts') ? createTtsPipeline() : null;
        if (ttsPipe) ttsPipeline = ttsPipe;

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
                                // Accumulate cho history + feed pipeline (tach cau, phat dan).
                                ttsBuffer += (e.data.delta || '');
                                if (ttsPipe) ttsPipe.feed(e.data.delta || '');
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
                            if (ttsPipe) { ttsPipe.cancel(); ttsPipe = null; }
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
            if (sawError) { removeTyping(); if (MODE === 'voice') restartListening(); return; }

            // TTS mode (hoac Voice PTT -> reply doc to): text da accumulate trong ttsBuffer
            if (responseMode === 'tts') {
                var ttsCleanText = ttsBuffer.replace(/\s*---SUGGEST---[\s\S]*$/, '').trim();
                if (!ttsCleanText) {
                    if (ttsPipe) { ttsPipe.cancel(); ttsPipe = null; }
                    removeTyping();
                    appendMsg('bot', 'Xin lỗi, tôi chưa có câu trả lời cho câu hỏi này.');
                    history.push({ role: 'assistant', content: '' });
                    if (MODE === 'voice') restartListening();
                    return;
                }
                history.push({ role: 'assistant', content: ttsCleanText });
                // Pipeline da phat dan trong luc stream; end() flush cau cuoi + gan nav/suggestions.
                if (ttsPipe) ttsPipe.end(navTarget, navName, doneSuggestions);
                else speakAndRevealSynced(ttsCleanText, navTarget, navName, doneSuggestions);
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
            if (ttsPipe) { ttsPipe.cancel(); ttsPipe = null; }
            removeTyping();
            setBusy(false);
            if (!sawError) appendMsg('error', 'Lỗi mạng: ' + e.message);
            if (MODE === 'voice') restartListening();
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
    var pttState = 'idle';  // idle | ready | listening (VAD dang nghe) | processing (STT+AI)
    var pttStream = null;
    var pttAudioCtx = null;
    var pttSourceNode = null;
    var pttProcessorNode = null;
    var pttSamples = [];   // Float32Array chunks captured tu ScriptProcessor
    // pttRecorder giu lai de pttCleanup khong ref undefined (legacy MediaRecorder)
    var pttRecorder = null;
    var pttChunks = [];

    // ---- VAD (voice activity detection) tuning cho che do Voice hands-free ----
    var VAD_ABS_MIN        = 0.012;   // nguong RMS toi thieu coi la tieng noi
    var VAD_FACTOR         = 2.5;     // RMS phai vuot noiseFloor x lan nay moi tinh la noi
    var VAD_END_SILENCE_MS = 1500;    // im lang bao lau -> coi nhu noi xong (user yc ~3s; 1.5s muot hon, doi tai day)
    var VAD_MIN_SPEECH_MS  = 350;     // luot noi ngan hon nay -> coi la nhieu, bo qua
    var VAD_MAX_UTTER_MS   = 15000;   // chong noi qua dai
    var VAD_IDLE_MS        = 25000;   // mo mic ma khong noi gi sau ngan nay -> tu tat phien
    var VAD_PREROLL_FRAMES = 3;       // so frame giu lai truoc khi phat hien noi (tranh cut am dau)
    // VAD runtime state
    var vadSpeaking = false, vadEnding = false;
    var vadNoiseFloor = 0.012, vadLevel = 0;
    var vadLastVoiceMs = 0, vadSpeechStartMs = 0, vadListenStartMs = 0;
    var vadPreroll = [];
    var waveRaf = null;
    var voiceWatchdog = null;
    var voiceStarting = false;          // chong double-start (race giua cac restartListening async)
    var voiceAutoTurns = 0;             // dem so luot TU DONG mo lai lien tiep (reset khi user bam mic)
    var VOICE_MAX_AUTO_TURNS = 20;      // qua nguong -> tam dung phien (chong goi STT/LLM/TTS vo han do nhieu/loi)

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
        voiceStarting = false;   // doi state = pttStartListening da xong/thoat -> mo khoa chong double-start
        pttBtn.classList.remove('is-recording', 'is-processing');
        if (state !== 'listening') stopWave();
        if (state === 'ready') {
            pttBtn.innerHTML = '<i class="fa fa-microphone"></i>';
            pttBtn.title = 'Bấm để bắt đầu trò chuyện bằng giọng nói';
            voiceStatus.innerHTML = '<i class="fa fa-info-circle"></i> Bấm mic để bắt đầu trò chuyện — bạn nói, mình tự nhận khi bạn dừng';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status';
        } else if (state === 'listening') {
            pttBtn.classList.add('is-recording');
            pttBtn.innerHTML = '<i class="fa fa-stop"></i>';
            pttBtn.title = 'Bấm để dừng trò chuyện';
            voiceStatus.className = 'v360cb__voice-status is-active';
            voiceStatus.style.display = 'flex';
            voiceStatus.innerHTML = '';
            buildWave(voiceStatus);
            var lbl = document.createElement('span');
            lbl.className = 'v360cb__wave-label';
            lbl.textContent = 'Đang nghe… cứ nói tự nhiên';
            voiceStatus.appendChild(lbl);
            startWave();
        } else if (state === 'processing') {
            pttBtn.classList.add('is-processing');
            pttBtn.innerHTML = '<i class="fa fa-circle-notch fa-spin"></i>';
            pttBtn.title = 'Đang xử lý';
            voiceStatus.innerHTML = '<i class="fa fa-cog fa-spin"></i> Đang xử lý...';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status is-connecting';
        }
    }

    function pttCleanup() {
        if (voiceWatchdog) { clearTimeout(voiceWatchdog); voiceWatchdog = null; }
        stopCapture();
        pttSamples = []; vadPreroll = []; vadSpeaking = false; vadEnding = false; vadLevel = 0;
        voiceStarting = false; voiceAutoTurns = 0;   // mo khoa start + reset dem loop khi roi/dong phien
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

    // ---- VAD helpers ----
    function rmsOf(buf) {
        var s = 0; for (var i = 0; i < buf.length; i++) s += buf[i] * buf[i];
        return Math.sqrt(s / buf.length);
    }
    function mergeSamples(chunks) {
        var t = 0, i; for (i = 0; i < chunks.length; i++) t += chunks[i].length;
        var m = new Float32Array(t), o = 0;
        for (i = 0; i < chunks.length; i++) { m.set(chunks[i], o); o += chunks[i].length; }
        return m;
    }
    // Cat im lang dau + cuoi (giu margin ~120ms) dua tren bien do tuong doi voi peak.
    function trimSilence(samples, sr) {
        var n = samples.length, i, peak = 0;
        for (i = 0; i < n; i++) { var a = samples[i] < 0 ? -samples[i] : samples[i]; if (a > peak) peak = a; }
        if (peak < 0.02) return samples;            // gan nhu im lang -> de nguyen
        var thr = Math.max(0.01, peak * 0.08);
        var start = 0, end = n - 1;
        while (start < n && (samples[start] < 0 ? -samples[start] : samples[start]) < thr) start++;
        while (end > start && (samples[end] < 0 ? -samples[end] : samples[end]) < thr) end--;
        var margin = Math.floor(sr * 0.12);
        start = Math.max(0, start - margin);
        end = Math.min(n - 1, end + margin);
        if (end <= start) return samples;
        return samples.subarray(start, end + 1);
    }

    // ---- Song am thanh (waveform meter) hien khi dang nghe ----
    var WAVE_BARS = 7;
    var waveBarEls = [];
    function buildWave(container) {
        var w = document.createElement('div');
        w.className = 'v360cb__wave';
        waveBarEls = [];
        for (var i = 0; i < WAVE_BARS; i++) {
            var b = document.createElement('span');
            b.className = 'v360cb__wave-bar';
            w.appendChild(b);
            waveBarEls.push(b);
        }
        container.appendChild(w);
    }
    function startWave() {
        if (waveRaf) return;
        (function loop() {
            waveRaf = requestAnimationFrame(loop);
            for (var i = 0; i < waveBarEls.length; i++) {
                var jitter = 0.45 + Math.random() * 0.55;   // nhap nhay cho song dong
                var h = 4 + vadLevel * 26 * jitter;
                waveBarEls[i].style.height = h.toFixed(1) + 'px';
            }
            vadLevel *= 0.9;   // tu tat dan khi im lang
        })();
    }
    function stopWave() {
        if (waveRaf) { cancelAnimationFrame(waveRaf); waveRaf = null; }
        waveBarEls = [];
    }

    // ---- Giai phong mic + audio nodes ----
    function stopCapture() {
        stopWave();
        try { if (pttProcessorNode) { pttProcessorNode.disconnect(); pttProcessorNode.onaudioprocess = null; } } catch (_) {}
        try { if (pttSourceNode) pttSourceNode.disconnect(); } catch (_) {}
        if (pttStream) { try { pttStream.getTracks().forEach(function (t) { t.stop(); }); } catch (_) {} }
        if (pttAudioCtx) { try { pttAudioCtx.close(); } catch (_) {} }
        pttProcessorNode = null; pttSourceNode = null; pttAudioCtx = null; pttStream = null;
    }

    // Defer ket thuc ra ngoai onaudioprocess (tranh disconnect node ngay trong callback cua chinh no).
    function scheduleEnd(fn) {
        if (pttState !== 'listening' || vadEnding) return;
        vadEnding = true;
        setTimeout(fn, 0);
    }

    // ---- VAD callback: do RMS -> phat hien noi / im lang -> tu ket thuc luot ----
    function vadProcess(e) {
        if (pttState !== 'listening') return;
        var input = e.inputBuffer.getChannelData(0);
        var frame = new Float32Array(input);   // phai copy (buffer dung chung)
        var rms = rmsOf(frame);
        var nowMs = pttAudioCtx.currentTime * 1000;

        // Muc do cho waveform (0..1): bat nhanh, nha cham
        var norm = Math.min(1, rms / 0.2);
        vadLevel = norm > vadLevel ? norm : (vadLevel * 0.6 + norm * 0.4);

        var thr = Math.max(VAD_ABS_MIN, vadNoiseFloor * VAD_FACTOR);
        if (rms > thr) {
            if (!vadSpeaking) {
                vadSpeaking = true;
                vadSpeechStartMs = nowMs;
                for (var p = 0; p < vadPreroll.length; p++) pttSamples.push(vadPreroll[p]);  // preroll -> khong cut am dau
                vadPreroll = [];
            }
            vadLastVoiceMs = nowMs;
            pttSamples.push(frame);
            if (nowMs - vadSpeechStartMs >= VAD_MAX_UTTER_MS) scheduleEnd(endUtterance);
        } else {
            vadNoiseFloor = vadNoiseFloor * 0.95 + rms * 0.05;   // hoc nen nhieu khi im lang
            if (vadSpeaking) {
                pttSamples.push(frame);   // giu duoi (se trim sau)
                if (nowMs - vadLastVoiceMs >= VAD_END_SILENCE_MS) {
                    if (vadLastVoiceMs - vadSpeechStartMs >= VAD_MIN_SPEECH_MS) scheduleEnd(endUtterance);
                    else { vadSpeaking = false; pttSamples = []; }   // qua ngan -> bo, nghe tiep
                }
            } else {
                vadPreroll.push(frame);
                if (vadPreroll.length > VAD_PREROLL_FRAMES) vadPreroll.shift();
                if (nowMs - vadListenStartMs >= VAD_IDLE_MS) scheduleEnd(function () { stopVoiceSession(true); });
            }
        }
    }

    async function pttStartListening() {
        if (pttState === 'listening' || voiceStarting) return;   // chong goi chong (race async getUserMedia)
        voiceStarting = true;
        if (typeof stopTtsAudio === 'function') stopTtsAudio();
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            appendMsg('error', 'Trình duyệt không hỗ trợ microphone (cần HTTPS hoặc localhost).');
            pttSetState('ready'); return;
        }
        var Ctx = window.AudioContext || window.webkitAudioContext;
        if (!Ctx) { appendMsg('error', 'Trình duyệt không hỗ trợ Web Audio API.'); pttSetState('ready'); return; }
        try {
            pttStream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    channelCount: { ideal: 1 }, sampleRate: { ideal: 16000 },
                    echoCancellation: true, noiseSuppression: true, autoGainControl: true
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
            appendMsg('error', msg); pttSetState('ready'); return;
        }
        try { pttAudioCtx = new Ctx({ sampleRate: 16000 }); }
        catch (_) { try { pttAudioCtx = new Ctx(); } catch (e2) {
            appendMsg('error', 'Không khởi tạo AudioContext: ' + e2.message); stopCapture(); pttSetState('ready'); return;
        }}
        try {
            pttSourceNode = pttAudioCtx.createMediaStreamSource(pttStream);
            pttProcessorNode = pttAudioCtx.createScriptProcessor(2048, 1, 1);
        } catch (e) {
            appendMsg('error', 'Không khởi tạo Audio nodes: ' + e.message); stopCapture(); pttSetState('ready'); return;
        }
        // Reset VAD state
        pttSamples = []; vadPreroll = []; vadSpeaking = false; vadEnding = false;
        vadNoiseFloor = 0.012; vadLevel = 0;
        vadListenStartMs = pttAudioCtx.currentTime * 1000;
        vadLastVoiceMs = vadListenStartMs; vadSpeechStartMs = vadListenStartMs;
        pttProcessorNode.onaudioprocess = vadProcess;
        pttSourceNode.connect(pttProcessorNode);
        pttProcessorNode.connect(pttAudioCtx.destination);  // can connect de callback fire (Chrome quirk)
        pttSetState('listening');
    }

    function armVoiceWatchdog() {
        if (voiceWatchdog) clearTimeout(voiceWatchdog);
        // Backstop: neu ket o 'processing' (vd pipeline treo) -> mo lai mic sau 45s.
        // Chi mo neu KHONG con audio TTS dang phat (ttsAudioEl null) de tranh thu lai giong AI.
        voiceWatchdog = setTimeout(function () {
            if (MODE === 'voice' && pttState === 'processing' && !ttsAudioEl) restartListening();
        }, 45000);
    }

    // Ket thuc 1 luot noi (VAD trigger): cat im lang -> encode -> upload STT.
    function endUtterance() {
        if (pttState !== 'listening') return;
        var sr = pttAudioCtx ? pttAudioCtx.sampleRate : 16000;
        var chunks = pttSamples; pttSamples = [];
        pttSetState('processing');
        armVoiceWatchdog();
        stopCapture();
        var merged = mergeSamples(chunks);
        if (!merged.length) { restartListening(); return; }
        var trimmed = trimSilence(merged, sr);
        var durationSeconds = trimmed.length / sr;
        if (durationSeconds < 0.25) { restartListening(); return; }   // qua ngan -> bo, nghe lai
        var normalized = pttNormalizeForStt(trimmed);
        console.log('[v360cb-ptt] VAD utter ' + trimmed.length + ' samples @' + sr + 'Hz = '
            + durationSeconds.toFixed(2) + 's, gain x' + normalized.stats.gain.toFixed(2));
        var wavBlob = encodeWav(normalized.samples, sr);
        pttUpload(wavBlob, sr, durationSeconds, normalized.stats);
    }

    // Dung han phien thoai (bam mic de dung, hoac im lang qua lau).
    function stopVoiceSession(isIdle) {
        if (voiceWatchdog) { clearTimeout(voiceWatchdog); voiceWatchdog = null; }
        stopCapture();
        pttSetState('ready');
        if (isIdle) {
            voiceStatus.innerHTML = '<i class="fa fa-info-circle"></i> Tạm dừng (im lặng lâu). Bấm mic để nói tiếp.';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status';
        }
    }

    // Mo lai mic sau khi AI tra loi xong (vong lap hoi thoai).
    function restartListening() {
        if (voiceWatchdog) { clearTimeout(voiceWatchdog); voiceWatchdog = null; }
        if (MODE !== 'voice') return;
        if (!stage.classList.contains('cb-open')) { stopCapture(); pttSetState('ready'); return; }
        // Chong loop vo han: moi lan TU DONG mo lai dem +1; qua nguong -> tam dung, doi user chu dong bam mic.
        // (Tieng on/loi lien tuc co the lap STT/LLM/TTS; idle 25s chi tu dung khi IM LANG, khong dung khi co on.)
        voiceAutoTurns++;
        if (voiceAutoTurns >= VOICE_MAX_AUTO_TURNS) {
            stopVoiceSession(false);
            voiceStatus.innerHTML = '<i class="fa fa-info-circle"></i> Tạm dừng để tránh lặp liên tục. Bấm mic để nói tiếp.';
            voiceStatus.style.display = 'flex';
            voiceStatus.className = 'v360cb__voice-status';
            return;
        }
        vadEnding = false;
        pttStartListening();
    }

    function pttUpload(blob, sampleRate, durationSeconds, gainStats) {
        if (!STT_URL) {
            appendMsg('error', 'STT URL chưa cấu hình.');
            pttSetState('ready'); return;
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
                if (!j.ok) {
                    var err = new Error(j.error || 'STT fail');
                    err.limited = !!j.limited;   // het gioi han (khac loi ky thuat)
                    throw err;
                }
                var transcript = (j.transcript || '').trim();
                if (!transcript) {
                    // Khong nhan dien duoc -> nghe lai (hands-free), khong lam phien bang loi do.
                    console.warn('[v360cb-ptt] empty transcript -> nghe lai');
                    restartListening(); return;
                }
                console.log('[v360cb-ptt] transcript:', transcript);
                // send() hien transcript len bubble user. Voice mode -> effectiveMode()='tts' -> reply doc to.
                // Sau khi AI doc xong (finalizeAll) se tu mo lai mic; giu trang thai 'processing' den luc do.
                inputEl.value = transcript;
                inputEl.disabled = false;
                send();
                inputEl.disabled = true;
            })
            .catch(function (e) {
                console.warn('[v360cb-ptt]', e);
                if (e && e.limited) {
                    // Het gioi han -> bao dung sac thai + TAM DUNG phien (khong tu mo lai mic -> tranh lap).
                    appendMsg('error', '🎙 ' + e.message);
                    stopVoiceSession(false);
                } else {
                    appendMsg('error', '🎙 Lỗi nhận giọng nói: ' + e.message);
                    restartListening();   // loi tam thoi -> nghe lai; chi reopen khi user noi tiep
                }
            });
    }

    pttBtn.addEventListener('click', function () {
        if (pttState === 'listening' || pttState === 'processing') stopVoiceSession(false);
        else { vadEnding = false; voiceAutoTurns = 0; pttStartListening(); }   // bam mic = chu dong -> reset dem loop
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

    var ttsPipeline = null;   // pipeline TTS dang chay (phat tung cau)
    function stopTtsAudio() {
        if (ttsPipeline) { try { ttsPipeline.cancel(); } catch(_) {} ttsPipeline = null; }
        if (ttsRevealTicker) { clearInterval(ttsRevealTicker); ttsRevealTicker = null; }
        if (ttsAudioEl) { try { ttsAudioEl.pause(); ttsAudioEl.srcObject = null; } catch(_) {} ttsAudioEl = null; }
        setKuulaMuted(false);  // restore Kuula audio
    }

    // ====== TTS PIPELINE (tach cau + phat tuan tu + prefetch) ======
    // VBee TTS KHONG ho tro streaming -> gia-streaming: tach cau, phat cau dau ngay,
    // render cac cau sau song song (prefetch). Tich hop voi LLM stream qua feed()/end().
    var TTS_PREFETCH = 2;   // so cau fetch truoc song song

    function fetchSentenceAudio(text, _attempt) {
        _attempt = _attempt || 0;
        return fetch(TTS_URL, {
            method: 'POST', credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text: text })
        }).then(function (r) {
            if (!r.ok) return r.text().then(function (b) {
                var err = new Error('TTS ' + r.status + ': ' + String(b).substring(0, 120));
                err.status = r.status;   // 429 = het gioi han doc
                throw err;
            });
            return r.blob();
        }).then(function (blob) {
            // Server doi khi tra 200 voi body rong (loi VBee transient) -> coi nhu that bai de retry.
            if (!blob || blob.size === 0) throw new Error('TTS empty audio');
            return URL.createObjectURL(blob);
        }).catch(function (e) {
            // Het gioi han (429) -> retry vo nghia + ton quota -> nem luon de pipeline bao 1 lan.
            if (e && e.status === 429) throw e;
            // Retry tu tu: tranh 1 cau loi transient (timeout/queue) lam pipeline bo audio + "phun" text.
            if (_attempt < 2) {
                console.warn('[v360cb-tts] retry seg fetch #' + (_attempt + 1) + ': ' + e.message);
                return new Promise(function (res) { setTimeout(res, 400 * (_attempt + 1)); })
                    .then(function () { return fetchSentenceAudio(text, _attempt + 1); });
            }
            throw e;
        });
    }

    function createTtsPipeline() {
        var segs = [];           // { raw, tts, audio: Promise<blobUrl|null> }
        var rawBuf = '', consumed = 0, speakBuf = '';
        var streamEnded = false, cancelled = false, playing = false, finalized = false;
        var playIndex = 0, fetchUpto = 0;
        var bubble = null, revealedBase = '', fallbackFull = '', warnedAutoplay = false, warnedLimit = false;
        var endMeta = { navTarget: null, navName: null, suggestions: null };

        function ensureBubble() { if (!bubble) { removeTyping(); bubble = createStreamingBubble(); } }

        // Tach 1 cau hoan chinh tu speakBuf (boundary: . ! ? … + space, hoac newline).
        function popSentence(force) {
            if (!speakBuf) return null;
            var m = /[.!?…]\s|\n/.exec(speakBuf);
            if (m) {
                var isNl = m[0] === '\n';
                var keepEnd = isNl ? m.index : m.index + 1;
                var skipTo  = isNl ? m.index + 1 : m.index + 2;
                var s = speakBuf.slice(0, keepEnd).trim();
                speakBuf = speakBuf.slice(skipTo);
                return s;
            }
            if (force) { var rest = speakBuf.trim(); speakBuf = ''; return rest; }
            return null;
        }

        function drain(force) {
            var s;
            while ((s = popSentence(force)) !== null) {
                if (s) {
                    var tts = cleanTextForTts(s);
                    if (tts) segs.push({ raw: s, tts: tts, audio: null });
                }
            }
            schedulePrefetch();
            if (!playing) playNext();
        }

        function schedulePrefetch() {
            var upto = Math.min(segs.length, playIndex + TTS_PREFETCH + 1);
            for (; fetchUpto < upto; fetchUpto++) startFetch(segs[fetchUpto]);
        }
        function startFetch(seg) {
            if (!seg || seg.audio) return;
            seg.audio = fetchSentenceAudio(seg.tts).catch(function (e) {
                // Het gioi han doc -> bao 1 lan/cau tra loi, roi tiep tuc hien text khong audio.
                if (e && e.status === 429 && !warnedLimit) {
                    warnedLimit = true;
                    appendMsg('error', '🔊 Đã đạt giới hạn đọc to (TTS) — câu trả lời chỉ hiển thị bằng văn bản.');
                }
                console.warn('[v360cb-tts] seg fetch fail', e); return null;
            });
        }

        function revealSeg(seg, progress) {
            if (cancelled || finalized) return;   // da ket thuc -> khong render lai (tranh "1 cuc" roi ve)
            ensureBubble();
            var n = Math.ceil(seg.raw.length * Math.max(0, Math.min(1, progress)));
            var sep = (revealedBase && !/\s$/.test(revealedBase)) ? ' ' : '';
            bubble.setText(revealedBase + sep + seg.raw.substring(0, n));
        }
        function commitSeg(seg) {
            var sep = (revealedBase && !/\s$/.test(revealedBase)) ? ' ' : '';
            revealedBase += sep + seg.raw;
        }

        function playNext() {
            if (cancelled) return;
            if (playIndex >= segs.length) {
                playing = false;
                if (streamEnded) finalizeAll(false);
                return;
            }
            playing = true;
            var seg = segs[playIndex];
            schedulePrefetch();
            startFetch(seg);
            seg.audio.then(function (blobUrl) {
                if (cancelled) return;
                if (!blobUrl) {                 // fetch loi -> reveal text, bo qua audio
                    revealSeg(seg, 1); commitSeg(seg);
                    playIndex++; playNext(); return;
                }
                ensureBubble();
                var a = new Audio(blobUrl);
                ttsAudioEl = a;
                var ticker = null;
                a.addEventListener('playing', function () { setKuulaMuted(true); });
                a.addEventListener('loadedmetadata', function () {
                    if (ticker) return;
                    ticker = setInterval(function () {
                        if (cancelled || !a.duration || !isFinite(a.duration)) return;
                        revealSeg(seg, a.currentTime / a.duration);
                    }, 60);
                    ttsRevealTicker = ticker;
                });
                function nextSeg() {
                    if (ticker) { clearInterval(ticker); ticker = null; ttsRevealTicker = null; }
                    if (cancelled || finalized) return;
                    revealSeg(seg, 1); commitSeg(seg);
                    try { URL.revokeObjectURL(blobUrl); } catch (_) {}
                    playIndex++; playNext();
                }
                a.addEventListener('ended', nextSeg);
                a.addEventListener('error', function () { if (!cancelled) nextSeg(); });
                a.play().catch(function () {
                    if (ticker) { clearInterval(ticker); ticker = null; }
                    autoplayBlocked();
                });
            });
        }

        // Browser chan autoplay -> bo audio, reveal het text con lai, ket thuc.
        function autoplayBlocked() {
            if (cancelled || finalized) return;
            var rest = revealedBase;
            for (var i = playIndex; i < segs.length; i++) {
                var sep = (rest && !/\s$/.test(rest)) ? ' ' : '';
                rest += sep + segs[i].raw;
            }
            revealedBase = rest;
            cancelled = true;
            finalizeAll(true);
            if (!warnedAutoplay) {
                warnedAutoplay = true;
                appendMsg('error', 'Trình duyệt chặn auto-play audio. Bấm vào tab này 1 lần rồi thử lại.');
            }
        }

        function finalizeAll(skipKuula) {
            if (finalized) return;
            finalized = true;
            if (ttsRevealTicker) { clearInterval(ttsRevealTicker); ttsRevealTicker = null; }
            ensureBubble();
            bubble.setText(revealedBase || fallbackFull);
            bubble.finalize(endMeta.navTarget, endMeta.navName);
            if (endMeta.suggestions) renderSuggestions(endMeta.suggestions);
            if (endMeta.navTarget) setTimeout(function () { tryNavigate(endMeta.navTarget, endMeta.navName); }, 400);
            if (!skipKuula) setKuulaMuted(false);
            ttsAudioEl = null;
            cancelled = true;   // terminal: chan moi reveal/audio con sot lai sau finalize
            // Voice hands-free: AI doc xong -> mo lai mic cho user noi tiep.
            if (MODE === 'voice') setTimeout(restartListening, 200);
        }

        return {
            // Nhan text delta tu LLM stream -> tach cau dan + bat dau TTS/phat ngay.
            feed: function (delta) {
                if (cancelled) return;
                rawBuf += (delta || '');
                var mi = rawBuf.indexOf('---SUGGEST---');
                // Chua thay marker: giu lai 16 ky tu cuoi phong marker '---SUGGEST---' dang hinh thanh.
                var speakableEnd = mi >= 0 ? mi : Math.max(consumed, rawBuf.length - 16);
                if (speakableEnd > consumed) {
                    speakBuf += rawBuf.slice(consumed, speakableEnd);
                    consumed = speakableEnd;
                }
                drain(false);
            },
            // LLM stream xong -> flush cau cuoi, gan nav/suggestions, ket thuc.
            end: function (navTarget, navName, suggestions) {
                if (cancelled) return;
                endMeta = { navTarget: navTarget || null, navName: navName || null, suggestions: suggestions || null };
                streamEnded = true;
                var mi = rawBuf.indexOf('---SUGGEST---');
                var speakableEnd = mi >= 0 ? mi : rawBuf.length;
                if (speakableEnd > consumed) { speakBuf += rawBuf.slice(consumed, speakableEnd); consumed = speakableEnd; }
                fallbackFull = (mi >= 0 ? rawBuf.slice(0, mi) : rawBuf).trim();
                drain(true);
                if (segs.length === 0) { finalizeAll(false); return; }
                if (!playing) playNext();
                // Safety: giong async (-phg) co the mat nhieu giay (POST + poll) cho audio cau dau.
                // Chi khi sau 15s VAN chua phat duoc cau nao -> coi nhu audio loi -> hien text + ket thuc.
                // (finalizeAll set cancelled=true nen audio toi muon se khong render lai "1 cuc".)
                setTimeout(function () {
                    if (!cancelled && !finalized && !bubble) { revealedBase = fallbackFull; finalizeAll(false); }
                }, 15000);
            },
            cancel: function () {
                cancelled = true;
                if (ttsRevealTicker) { clearInterval(ttsRevealTicker); ttsRevealTicker = null; }
                if (ttsAudioEl) { try { ttsAudioEl.pause(); } catch (_) {} ttsAudioEl = null; }
                segs.forEach(function (sg) {
                    if (sg.audio) sg.audio.then(function (u) { if (u) { try { URL.revokeObjectURL(u); } catch (_) {} } });
                });
                setKuulaMuted(false);
            }
        };
    }

    // Fallback 1-phat (khi khong stream): day toan bo text qua pipeline.
    function speakAndRevealSynced(fullText, navTarget, navName, suggestions) {
        if (!TTS_URL || !fullText) {
            removeTyping();
            var b = createStreamingBubble();
            b.setText(fullText); b.finalize(navTarget, navName);
            if (suggestions) renderSuggestions(suggestions);
            if (navTarget) setTimeout(function () { tryNavigate(navTarget, navName); }, 700);
            return;
        }
        stopTtsAudio();
        var p = createTtsPipeline();
        ttsPipeline = p;
        p.feed(fullText);
        p.end(navTarget, navName, suggestions);
    }

    console.log('[v360cb] widget mounted, session=' + SESSION_GUID);
})();
