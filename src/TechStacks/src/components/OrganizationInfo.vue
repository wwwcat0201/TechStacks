<template>
  <div class="org-info">

    <v-card v-if="organization.descriptionHtml" class="org-description">
      <v-card-title v-html="organization.descriptionHtml"></v-card-title>
    </v-card>

    <v-card v-if="isAuthenticated" class="org-subscribe">
      <v-card-title>

        <v-layout v-if="!showSubscription" justify-center>
          <v-btn v-if="subscribedPostTypes.length == 0" @click="showSubscription=true" color="primary">
            Subscribe to {{ organization.name.length > 15 ? 'Organization' : organization.name }}
          </v-btn>
          <v-btn v-else @click="showSubscription=true">
            Subscribed
          </v-btn>
        </v-layout>

        <v-layout v-if="showSubscription" column>
          <p>
            Receive notifications for new:
          </p>
          <v-checkbox v-model="postTypes" v-for="postType in browsablePostTypes" :key="postType"
                      :label="postTypeLabel(postType)" :value="postType"></v-checkbox>

          <v-btn v-if="showSubscription" @click="submit()" :disabled="loading">Update Subscription</v-btn>
        </v-layout>

      </v-card-title>
    </v-card>
  </div>
</template>

<script>
import { mapGetters } from "vuex";
import { subscribeToOrganization } from "~/shared/gateway";

export default {
  props: ['organization','show'],
  computed: {
    subscribedPostTypes() {
      const orgSub = this.getOrganizationSubscription(this.organization.id);
      return orgSub && orgSub.postTypes || []
    },

    ...mapGetters([
      "loading",
      "isAuthenticated",
      "browsablePostTypes",
      "postTypeLabelsMap",
      "getOrganizationSubscription"
    ])
  },

  methods: {

    async submit() {
      try {
        this.$store.commit('loading', true);

        const unmodified = this.postTypes.length == this.originalPostTypes.length && 
                           this.postTypes.every(x => this.originalPostTypes.indexOf(x) >= 0);
        if (!unmodified)
        {
          await subscribeToOrganization(this.organization.id, this.postTypes);
          this.$store.dispatch('loadUserOrganizations');
          this.originalPostTypes = Array.from(this.postTypes);
        }

        this.showSubscription = false;
      } catch(e) {
        this.responseStatus = e.responseStatus || e;
      } finally {
        this.$store.commit('loading', false);
      }
    },

    postTypeLabel(postType) {
      return this.postTypeLabelsMap[postType];
    },
  },

  mounted() {
    const orgSub = this.getOrganizationSubscription(this.organization.id);
    this.postTypes = (orgSub
      ? orgSub.postTypes
      : this.organization.defaultSubscriptionPostTypes) || [];
    this.originalPostTypes = Array.from(this.postTypes);
    this.showSubscription = this.show === 'subscription';
  },
  
  data: () => ({
    showSubscription: false,
    postTypes: [],
    originalPostTypes: [],
  })
}
</script>

<style>
.org-description .gfm, .org-description .gfm p, .org-description .gfm blockquote, .org-description .gfm ul, .org-description .gfm ol, .org-description .gfm dl, .org-description .gfm table {
  font-size: 14px !important;
}
.org-info {
  margin: 0 0 .5em 0;
}
</style>
