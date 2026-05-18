import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SoundsService {
  // Hàm tạo âm thanh chung
  private playTone(frequency: number, type: OscillatorType, duration: number, volume: number = 0.5) {
    const audioCtx = new (window.AudioContext || (window as any).webkitAudioContext)();
    
    // 1. MỞ KHÓA TRÌNH DUYỆT: Ép trình duyệt đánh thức hệ thống âm thanh
    if (audioCtx.state === 'suspended') {
      audioCtx.resume();
    }

    const oscillator = audioCtx.createOscillator();
    const gainNode = audioCtx.createGain();

    oscillator.type = type;
    oscillator.frequency.value = frequency;
    
    // 2. TĂNG ÂM LƯỢNG: Sử dụng biến volume truyền vào (mặc định 50%)
    gainNode.gain.setValueAtTime(volume, audioCtx.currentTime);

    oscillator.connect(gainNode);
    gainNode.connect(audioCtx.destination);

    oscillator.start(audioCtx.currentTime);
    oscillator.stop(audioCtx.currentTime + duration);

    oscillator.onended = () => {
      audioCtx.close();
    };
  }

  // Tiếng bíp Thành công (Đã tăng âm lượng và kéo dài thêm 1 chút)
  playSuccess() {
    // Thông số: Tần số 1000Hz (Bíp thanh hơn), dạng sóng 'sine', dài 0.25 giây, Âm lượng 1.0 (Max)
    this.playTone(1000, 'sine', 0.25, 1.0); 
  }

  // Tiếng rè Thất bại
  playError() {
    // Thông số: Tần số 250Hz, dạng sóng 'sawtooth', dài 0.4 giây, Âm lượng 0.5
    this.playTone(250, 'sawtooth', 0.4, 1.0);
  }
}
