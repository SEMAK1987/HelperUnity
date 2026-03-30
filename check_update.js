import axios from 'axios';
import fs from 'fs-extra';
import path from 'path';

async function check() {
  const versionFile = path.join(process.cwd(), 'version.json');
  if (!await fs.pathExists(versionFile)) {
    console.log('Version file not found. Skipping update check.');
    return;
  }

  const localVersion = await fs.readJson(versionFile);
  console.log(`[UPDATE] Текущая версия: ${localVersion.version}`);

  try {
    // Simulated remote check
    const remoteVersion = "1.3.0";
    if (remoteVersion !== localVersion.version) {
      console.log(`[UPDATE] Доступна новая версия: ${remoteVersion}`);
      console.log(`[UPDATE] Пожалуйста, запустите приложение и нажмите кнопку 'Обновить' в интерфейсе.`);
    } else {
      console.log('[UPDATE] У вас установлена актуальная версия.');
    }
  } catch (error) {
    console.log('[UPDATE] Не удалось проверить обновления.');
  }
}

check();
