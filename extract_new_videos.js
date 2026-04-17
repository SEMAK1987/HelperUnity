const fs = require('fs');

const userRequest = `https://www.youtube.com/watch?v=hauVevcZj6k&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=2   https://www.youtube.com/watch?v=1Iv8Z00fhDY&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=3   https://www.youtube.com/watch?v=vflF2PBPfv4&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=4   https://www.youtube.com/watch?v=eW4HXqkHOh4&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=5   https://www.youtube.com/watch?v=NRUk7YzXyhE&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=6   https://www.youtube.com/watch?v=qKPnEi99aZQ&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=7    https://www.youtube.com/watch?v=KG_XBc2a6-4&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=8   https://www.youtube.com/watch?v=mDkjMNrQUio&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=9   https://www.youtube.com/watch?v=KLuyf9gqCik&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=10   https://www.youtube.com/watch?v=CByz9DnybHE&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=10&pp=iAQB   https://www.youtube.com/watch?v=GiUgQMkMI2o&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=12   https://www.youtube.com/watch?v=u8zJnXnZiFE&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=13   https://www.youtube.com/watch?v=f_48bi4qfsA&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=14   https://www.youtube.com/watch?v=28J7H2zKMa0&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=15   https://www.youtube.com/watch?v=Sso6hFBBG54&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=16   https://www.youtube.com/watch?v=MB-WZZxRWts&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=17   https://www.youtube.com/watch?v=8TO5Q-rwHLs&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=18   https://www.youtube.com/watch?v=2yH41kSFG8I&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=19   https://www.youtube.com/watch?v=imTfeYHrW84&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=20   https://www.youtube.com/watch?v=VGIkT9fPh7Y&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=21  https://www.youtube.com/watch?v=JBpxSQrZRXg&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=22   https://www.youtube.com/watch?v=k2_8NBxVlxk&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=23   https://www.youtube.com/watch?v=zZbcg-7JdaU&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=24   https://www.youtube.com/watch?v=OEFJQ1cB6IQ&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=25   https://www.youtube.com/watch?v=c0mBcTvIBZ4&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=26   https://www.youtube.com/watch?v=lOyb0_rFA1A&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=27   https://www.youtube.com/watch?v=WQ0Gf1Ncjuw&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=28   https://www.youtube.com/watch?v=CeTVy8WYWoc&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=29  https://www.youtube.com/watch?v=bT0D1uI_RNI&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=30   https://www.youtube.com/watch?v=_V-mZbmbKhQ&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=31   https://www.youtube.com/watch?v=qPM3Fm0gqGo&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=32   v=qPM3Fm0gqGo&list=PLAjqNiXIv5p3K8EpVm2cOYg23p_T9egsv&index=32`;

const urlRegex = /https:\/\/www\.youtube\.com\/watch\?v=[a-zA-Z0-9_-]+(&list=[a-zA-Z0-9_-]+)?(&index=\d+)?(&pp=[a-zA-Z0-9_-]+)?(&t=\d+s)?/g;
const urls = userRequest.match(urlRegex) || [];
const uniqueUrls = [...new Set(urls)];

console.log('Total URLs found:', urls.length);
console.log('Unique URLs found:', uniqueUrls.length);

const kb = JSON.parse(fs.readFileSync('knowledge_base.json', 'utf8'));
const existingUrls = new Set(kb.youtube_videos);
let addedCount = 0;

uniqueUrls.forEach(url => {
    if (!existingUrls.has(url)) {
        kb.youtube_videos.push(url);
        addedCount++;
    }
});

kb.version = "15.20.0";
// Update video count in features to reflect reality + some growth
kb.ai_modes_info.no_internet.features[1] = `${kb.youtube_videos.length}+ видео-уроков`;

fs.writeFileSync('knowledge_base.json', JSON.stringify(kb, null, 2));

console.log('Added', addedCount, 'new unique URLs.');
console.log('Total URLs now:', kb.youtube_videos.length);
console.log('Version updated to 15.20.0');
